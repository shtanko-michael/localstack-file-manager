# Simple file manager using LocalStack

**Make sure you have installed the required environment as I described in detail in [this article](https://medium.com/@shtanko.michael/mocking-aws-with-localstack-in-net-core-3-ef32ae888706)**

https://medium.com/@shtanko.michael/mocking-aws-with-localstack-in-net-core-3-ef32ae888706

## Quick start

```bash
# Clone/Download the repo
git clone https://github.com/shtanko-michael/localstack-file-manager.git
```

### Server app

* Open LocalStack.sln file in Visual Studio
* Run *Update-Database* in Package Manager Console to create the local SQLite database
* Run the project

### Client app

```bash
# move to the repo directory
cd localstack-file-manager/client

# install all dependencies with npm
npm i

# run the client app
npm run start
```

go to [http://localhost:3000](http://localhost:3000) (by default) in your browser

# License

[MIT](/LICENSE)