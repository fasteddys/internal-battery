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

## Deployment

Before deployment - verify that the **Azure Key Vault** is up to date with any new secrets that was added to user secrets or app settings.

### WebApp Secrets Checklist
* Authentication:AzureAdB2C:ClientSecret
* Braintree:MerchantID
* Braintree:PublicKey
* Braintree:PrivateKey
* redis:host
* Sendgrid:ApiKey
* SysEmail:ApiKey

### API Secrets Checklist
* Braintree:MerchantID
* Braintree:PublicKey
* Braintree:PrivateKey
* SysEmail:ApiKey
* Woz:AccessToken
* CareerCircleSqlConnection

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