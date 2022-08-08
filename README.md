# Jump to Links
* [Pre Build Requirements](#Pre-Build-Requirements)
  * [Required for build](Required-for-build)
* [Building](#Building)
* [Questions I would have asked and assumptions I made](#Questions-I-would-have-asked-and-assumptions-I-made)
  * [Required fields](#Required-fields)
  * [Uniqueness](#Uniqueness)
* [Architectural decisions](#Architectural-decisions)
  * [What's up with this "ServiceLayerResponse" thing?](#Whats-up-with-this-servicelayerresponse-thing)
  * [My approach to layers](#My-approach-to-layers)
  * [RequestAccessor](#RequestAccessor)

# Pre Build Requirements
As it is, this application expects a folder destination for Logs and Databases (Production, Development and Automated Tests).

If you wish to use the destinations already configured, just run the following commands in powershell:
```ps1
New-Item -Path "c:\" -Name "Coding_Challenge_Upperman" -ItemType "directory"
New-Item -Path "c:\Coding_Challenge_Upperman" -Name "Prod-DB" -ItemType "directory"
New-Item -Path "c:\Coding_Challenge_Upperman" -Name "Dev-DB" -ItemType "directory"
New-Item -Path "c:\Coding_Challenge_Upperman" -Name "Test-DB" -ItemType "directory"
New-Item -Path "c:\Coding_Challenge_Upperman" -Name "Log" -ItemType "directory"
$env:Coding_Challenge_Upperman = 'c:\Coding_Challenge_Upperman\Test-DB\'
```
> **Note that this expects you to have a C drive.**

Alternatively you can create your own folders, just know that you will need to update the following locations:
* appsettings.Development.Json line 8
* appsettings.Json line 8
* nlog.config line 6

> **Note that the integration tests will require you to have an environment variable with the desired directory for the integration database location.  That environment variable is named `Coding_Challenge_Upperman` and is referenced in the file LiteDbTestBase on line 15.**

## Required for build

You will need .NET Core 3.1 SDK and Runtime which can be found here: https://dotnet.microsoft.com/download/visual-studio-sdks

# Building

Once you have pulled, navigate in powershell to the contacts folder and run the command: `dotnet build Contacts.sln -o "./build" -c release `

Then, simply run the generated `API.exe` in the `build` folder.  The API will then be reachable at `localhost:5000`.

# Questions I would have asked and assumptions I made

## Required fields:

I wasn't sure if any fields were meant to be required, and that is something I should have asked.  Given I didn't have direct access to the Product Owner in this case I made the following assumptions:

1. Given that this application is used for storing contacts, the name field should be required.
  1 Given that Middle names are not universal, I made that field optional.
1. If a phone number is provided, then the type of phone number should also be there (and vice versa).

These assumptions are coded for and validated against in the Service layer.  Violating them returns a 400 error.

## Uniqueness:

I would have asked if any data was intended to be unique. I made the following assumptions:
1. Given that the `call-list` endpoint retrieves only records with a home phone included, and only returns one phone number in the record, then users should only be able to specify one phone number per type.

These assumptions are coded for and validated against in the Service layer.  Violating them returns a 400 error.

# Architectural decisions

I try to maintain unidirectional flow.  For context, here's the VS generated dependency graph: 
![Architecture view for Contacts With Tests](https://user-images.githubusercontent.com/56522001/134828137-3c470bdc-57ce-4a48-bd11-597e1cb55ddf.png)
<br/>
And here it is without the tests (Which are excluded from production builds)
<br/>
![Architecture view for Contacts_4](https://user-images.githubusercontent.com/56522001/134828238-ab2dd195-d7b1-4403-8c71-d8141f8df536.png)

This is one of the reasons for my usage of the `ServiceLayerResponse` data structure.

## What's up with this "ServiceLayerResponse" thing?

TL;DR: It provides a consistent data contract between the Services and clients (in this case the API) which the clients can then consume to determine how they want to provide that information to the enduser.

Our API needs to know certain things about the application in order to know what data it should return.  It's not good enough to know that creating a Contact failed, it needs to know if that was an authorization error, a data validation error, etc.   We could implement that logic inside of the API so that it knows exactly what's going on, but then what if we wanted to add a new client to the service?  Let's say we eventually wanted to have an MVC application which returned Razor views to the User so they could add Contacts using a form?  We would either have to refactor the logic back up the chain, or duplicate that logic in the MVC controllers.   The `ServiceLayerResponse` contract allows us to add multiple clients that consume the Service Layer without having to duplicate the logic.

## My approach to layers:

Each layer has it's own responsibilities:

* Client layer:
  * Authenticate User
  * Receive Data from the User
  * Format that Data for the Services
  * Retrieve Data from Services
  * Return Data to User in a way appropriate for it's context
* Services Layer:
  * Authorize request
  * Validate request
  * Handle business logic
  * Communicate with DataAccess layer
  * Communicate information back to Client Layer
* Data Access Layer:
  * Abstract away persistent storage mechanisms
  * CRUD operations

The last thing on the graph is "Models".  This is just a dumping ground for Plain objects.  This way we have a single dependency free project that everything else can reference without any issues.  You may notice that the API has it's own sets of models in the DTO's folder.   This is just a way to decouple user-facing interfaces from the internals of our application.   This way we can change the shapes of data within our application without having to push out a breaking change to our users.

## RequestAccessor

You may notice that the API Controller uses a class called `RequestAccessor` instead of just grabbing the TraceId from the Request directly. I wanted to be able to provide the TraceId to users upon uncaught exceptions so that they could report them to admins who could identify the issue in the logs.   In order to unit test that, I needed to abstract away the interactions with the Request object because it isn't easy to Mock.

