#Interview Project
This is functionally complete.  I've got a few wishlist items that I'd like to see in a project like this but I lack the time to continue digging.
* UI level automated integration tests
* Completed unit test coverage of the repository and domain layers
* log4net integration
* extend the influence of angular to transform the entire application into a SPA

#Requirements
You'll need VS >= 2012 in order to open the project. Nuget references have been added. You'll need to manually update your references or enable nuget auto-install for missing packages.

The thoughts that went into the current iteration follow:

## Unit Testing
Before I begin adding any new functionality I need tests to cover existing functionality to ensure that I'm not breaking any existing functionality.  Before I start adding test methods to the order controller I need to clean up some of the issues that would make the project untestable or unfriendly to complete testing. 

### Repository Interfaces
The first thing I'll tackle is the repository / data layer. The listed repository can't be mocked and there are hard dependencies in the controller on the repository classes.  I'll correct this by:

* Modifying the existing repository to fit into a simple generic repository pattern interface
* Add the other objects to the repository coverage while we're at it
* Add a UnitOfWork pattern and interface to the project to wrap the repositories
* Add the Castle Windsor DI framework to the project to inject dependencies and services into the controller(s)
* Modify the existing Order controller to use the DI injected services and repositories

### SMTP Service
There is a hard dependency on the system SMTP libraries.  Create a service that wraps this and inject it into the controller 

### Store Items In Repository
The items are new'ed up in the controller every time.  This isn't too realistic.  Lets add an application bootstrap function to Global.asax that inserts the items into the item repository on start.

### Add Unit Test Project, Rhino Mocks, MVCContrib
Used nuget to add references to Rhino Mocks and MVC Contrib libraries to enable complete testing.  Added tests to cover all existing actions and mocked out all dependent layers through constructor injection. This got 100% coverage on the controller layer, thus enabling development and refactoring

### Removed references to ViewData and Session where possible
direct usage of session storage and view data hurts scalability as it relies on weak abstractions to link server memory cache between server instances. Added angular to the orderindex view and removed almost every reference to ViewData/Session.

### Combine AddItemsToOrder and Save into one action, SubmitOrder
The current architecture for order management requires too many clicks and complicates the code.  The better approach is to build your order and submit it in one shot.

#### Loss of functionality
A user had the ability to place multiple line items in an order for the same item type.  The new implementation removes that capacity in favor of a more compact order entry form. In a real world situation I'd have gotten product sign-off on this first.

### Dropped Save and AddToOrder legacy actions
These actions are no longer in use so they have been removed and any associated unit test for these areas of the code have been dismantled
