# Career Circle

Upskilling people and getting them jobs!

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

What things you need to install the software and how to install them
 * git
 * .NET Core 2.1

**Note:**The WebApp is dependent on the API so you must have an API running in order to use the WebApp.

#### Setting Multiple Startup Projects in Visual Studio
 1. Go to **Solution Explorer** and view the project in **Solution** mode.
 2. Right Click solution **GarageP1** and click "Set Startup Projects"
 3. Select **Multiple start up projects**
 4. Select **Start** for both **UpDiddy** and **UpDiddyAPI**
 5. Order **UpDiddyAPI** 1st using the arrows
 6. Click **Apply** and **Ok**

### Installing

A step by step series of examples that tell you how to get a development env running

 1. `git clone (repo url)`
 2. Copy `appsettings.example.json` to `appsettings.Development.json` in `Product/API` and fill in the values.
 	* Alternatively you can utilize **User Secrets** via Visual Studio.
 3. Copy `appsettings.example.json` to `appsettings.Development.json` in `Product/WebApp` and fill in the values.
	* Alternatively you can utilize **User Secrets** via Visual Studio.


End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Development

### Client-Side Development

#### Bundling and Minification
All javascript files and css files are bundled and minified via [dotnet core](https://docs.microsoft.com/en-us/aspnet/core/client-side/bundling-and-minification?view=aspnetcore-2.2&tabs=visual-studio#configure-bundling-and-minification).

`site.dev.js` and `site.dev.css` are not minified and are used for development purposes. Production and Staging rely on `site.min.js` and `site.min.css`.

It is recommended to download the [Bundler & Minifier Plugin](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.BundlerMinifier) for Visual Studio to speed up development. This plugin triggers re-bundling automatically when the source file is changed as opposed to only triggering once during build.

#### Frontend Development
In order to compile the js/scss files you must have the following installed:
 * node 10.13.0 (or other compatible version)
 * npm 6.4.1+ (or other compatible version)

##### Steps to Compile
1. `cd Product/WebApp`
2. `npm install`
3. `npm run gulp compile` This will compile the scss, js and copy the fonts into wwwroot

#### Third-Party Libraries
Third-Party libraries are managed via [bower](https://bower.io/) and [npm](https://www.npmjs.com/). See the `bower.json` and `package.json` for libraries and versions included.

#### Steps to Install Bower Packages
1. `cd Product/WebApp`
2. `npm install` (installs bower as a dev dependency)
3. `npm run bower install`

Visual studio also supports Bower and should be able to install the dependencies via the GUI.

## Deployment

Before deployment - verify that the **Azure Key Vault** is up to date with any new secrets that was added to user secrets or app settings.

Be sure when configuring Key Vault secrets to use `--` as the seperator as `:` are not supported. The Application is setup to substitute out `--` for `:` when retreiving values.

### WebApp Secrets Checklist
* Authentication:AzureAdB2C:ClientSecret
* Braintree:MerchantID
* Braintree:PublicKey
* Braintree:PrivateKey
* redis:host
* Sendgrid:ApiKey
* SysEmail:ApiKey

### API Secrets Checklist
* AzureAdB2C:ClientSecret
* Braintree:MerchantID
* Braintree:PublicKey
* Braintree:PrivateKey
* SysEmail:ApiKey
* Woz:AccessToken
* CareerCircleSqlConnection
* Sovren:ServiceKey

### Key Vault Access
CareerCircle WebApp and API will utilize the **Azure Key Vault** for sensitive data such as passwords.

`Vault:ClientSecret` is **required as an App Setting** for both WebApp and API currently. **TODO** will be to utilize MSI to gain access to Azure Key Vault.


Add additional notes about how to deploy this on a live system

## Built With

* [.NET Core 2.1](https://docs.microsoft.com/en-us/dotnet/core/index) - Web and API Platform
* [Braintree](https://developers.braintreepayments.com/) - CC Processing
* [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/) - Secrets Management
* [Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/) - Identity Management Service
* [SendGrid](https://sendgrid.com/docs/for-developers/) - Email Delivery Service
* [WozU](https://woz-u.com/) - Training Resource Partner 1 (documentation available privately)
* [Hangfire](http://docs.hangfire.io/en/latest/) - Background Processing .NET Core
* Redis - Caching
* MsSQL - Database

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags).