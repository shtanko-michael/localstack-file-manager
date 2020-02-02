using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using LocalStack.DAL;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Amazon.CloudWatchLogs;
using Amazon.S3;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Formatting;
using System.IO;
using Serilog.Events;
using System;

namespace LocalStack {
    public class Startup {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment) {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            var appSettings = new AppSettings();
            Configuration.Bind(appSettings);
            services.AddSingleton(appSettings);

            services.AddDbContext<LocalStackContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            // mock Amazon services
            if (Environment.IsDevelopment()) {
                services.AddSingleton<IAmazonS3>(provider => {
                    var settings = provider.GetService<AppSettings>();
                    return new AmazonS3Client(new AmazonS3Config {
                        UseHttp = true,
                        ServiceURL = settings.Aws.S3.ServiceUrl,
                        ForcePathStyle = true
                    });
                });
                services.AddSingleton<IAmazonCloudWatchLogs>(provider => {
                    var settings = provider.GetService<AppSettings>();
                    return new AmazonCloudWatchLogsClient(new AmazonCloudWatchLogsConfig {
                        UseHttp = true,
                        ServiceURL = settings.Aws.CloudWatch.ServiceUrl
                    });
                });
            } else {
                services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
                services.AddAWSService<IAmazonS3>();
                services.AddAWSService<IAmazonCloudWatchLogs>();
            }

            services.AddControllers().AddNewtonsoftJson(options => {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters = new List<JsonConverter> {
                       new StringEnumConverter {
                           NamingStrategy = new CamelCaseNamingStrategy {
                               OverrideSpecifiedNames = true
                           }
                       }
                    };
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                JsonConvert.DefaultSettings = () => options.SerializerSettings;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IAmazonCloudWatchLogs cloudWatchLogs,
            AppSettings settings) {

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs.txt")
                .WriteTo.AmazonCloudWatch(new CloudWatchSinkOptions {
                    CreateLogGroup = true,
                    LogGroupName = settings.Aws.CloudWatch.GroupName,
                    TextFormatter = new AmazonCloudWatchTextFormatter(),
                    LogStreamNameProvider = new AmazonCloudWatchLogStreamNameProvider()
                }, cloudWatchLogs)
                .CreateLogger();

            app.UseCors(builder => builder.WithOrigins(settings.ClientUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }

    public class AmazonCloudWatchLogStreamNameProvider : ILogStreamNameProvider {
        public string GetLogStreamName() {
            var now = DateTime.UtcNow;
            return $"{now.Year}-{now.Month}-{now.Day+6}";
        }
    }

    public class AmazonCloudWatchTextFormatter : ITextFormatter {
        public void Format(LogEvent logEvent, TextWriter output) {
            logEvent.RenderMessage(output);
        }
    }
}
