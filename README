#Interview Project
I'll detail the thought process here. I was told to have some fun with this one so I'll tinker with it over the week and document the reasoning behind the work as I go.

## Unit Testing
Before I begin adding any new functionality I need tests to cover existing functionality to ensure that I'm not breaking any existing functionality.  Before I start adding test methods to the order controller I need to clean up some of the issues that would make the project untestable or unfriendly to complete testing. 

### RepositoryInterface Branch
The first thing I'll tackle is the repository / data layer. The listed repository can't be mocked and there are hard dependencies in the controller on the repository classes.  I'll correct this by:

* Modifying the existing repository to fit into a simple generic repository pattern interface
* Add the other objects to the repository coverage while we're at it
* Add a UnitOfWork pattern and interface to the project to wrap the repositories
* Add the Castle Windsor DI framework to the project to inject dependencies and services into the controller(s)
* Modify the existing Order controller to use the DI injected services and repositories
