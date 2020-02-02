import AppSettings from './appsettings';
import { env, Env } from 'models';
import devSettings from './appsettings.dev.json';
import prodSettings from './appsettings.prod.json';

const appSettings = new AppSettings(env == Env.dev ? devSettings : prodSettings);
export { AppSettings }
export default appSettings;