# ProphetsWay.BaseDataAccess

| Release   | Status |
|   ---     |  ---   |
| Latest Build: | [![Build status](https://dev.azure.com/ProphetsWay/ProphetsWay%20GitHub%20Projects/_apis/build/status/BaseDataAccess/BaseDataAccess%20CI)](https://dev.azure.com/ProphetsWay/ProphetsWay%20GitHub%20Projects/_build/?definitionId=7)
| Alpha:    | [![Build status](https://vsrm.dev.azure.com/ProphetsWay/_apis/public/Release/badge/dadb23ce-840b-4b7d-9783-dc5e9a2d9029/4/5)](https://dev.azure.com/ProphetsWay/ProphetsWay%20GitHub%20Projects/_release?definitionId=4)
| Beta:     | [![Build status](https://vsrm.dev.azure.com/ProphetsWay/_apis/public/Release/badge/dadb23ce-840b-4b7d-9783-dc5e9a2d9029/4/8)](https://dev.azure.com/ProphetsWay/ProphetsWay%20GitHub%20Projects/_release?definitionId=4)
| Release:  | [![Build status](https://vsrm.dev.azure.com/ProphetsWay/_apis/public/Release/badge/dadb23ce-840b-4b7d-9783-dc5e9a2d9029/4/9)](https://dev.azure.com/ProphetsWay/ProphetsWay%20GitHub%20Projects/_release?definitionId=4)


BaseDataAccess is a light library that is meant to help decouple your software's business logic projects from your Data Access Layer (DAL) implementation. 
With some new technologies, it is easy to allow yourself to create entity/models in your DAL and use them directly in your business layers, however this
makes it difficult to replace your DAL implementation if you decide to change how/what is hosting your data.  By adhering to a specific project dedicated to 
the models and interfaces required for your solution, you aren't restricted to the models that are created for you by your data layer implementation.

## Getting Started

BaseDataAccess is a lightweight project that consists of mostly just interfaces, but it also has a base abstract class you can utilize for 
convenience within your project.  This base class does use Reflection to work some magic, and if you're concerned about speed, you can override 
those virtual methods and manually evaluate them (or not use the base abstract class and just implement the interface on your own).

### Prerequisites

You can pull a copy of the source code from GitHub, or you can reference the library from NuGet.org from within Visual Studio.

```
Install-Package ProphetsWay.BaseDataAccess 
dotnet add package ProphetsWay.BaseDataAccess 
```

### Referencing and Using

The approach to use this project is that you want to create a new project in your solution to specify the data access requirements.  Generally my 
naming convention is "[ProjectName].DataAccess" and then the project that implements the actual data access layer is "[ProjectName].DataAccess.[ImplementationName]".
For "ImplementationName" I generally use a shortcut name to identify what the implementation is working with (ex: MSSQL, MySQL, Oracle, SQLite).


#### [ProjectName].DataAccess

In this project, I recommend the folder layout as follows:
root
 - Entities / Models Folder
 - IDaos Folder
 - I[ProjectName]DataAccess Class

##### Entities/Models

The Entities / Models folder name is really a personal preference on the naming convention.  The point is that you define the entities that you will be using
in your solution here.  Defining them outside of the DAL implementation allows you to use them however you see fit within your application.  Any new DAL implementation
will have to interact with the models defined here-in.  All of your entities must inherit the interface **IBaseEntity**.  This is to flag the entities as 
compatible with the other interfaces. 

##### IDaos

The IDaos folder is meant to specify what actions you want for each specific entity in your Entities folder.  Generally you should name your interfaces 
I[EntityName]Dao so that it's quick and easy to identify which files reference which entities.  Your interface should inherit from one of the base DAO interfaces.
Using one of the base interfaces will automatically include specific methods, however this DAO is where you will also specify any additional functionality 
that you need this entity to have.  (ex: ```IList<Customer> GetCustomersByCompanyId(int companyId); ``` to be specified on the ```ICustomerDao```)
All of the base interfaces are generic and require a type T to be specified, here you will specify the type of the entity this DAO is for.

```
public interface ICustomerDao : IBaseDao<Customer>
```

###### IBaseDao

IBaseDao specifies your basic CRUD calls: **Get**, **Insert**, **Update**, and **Delete**.  Each call does require a parameter of the type of entity to be passed
to it, even for Get and Delete where you might normally just need the "Id" of the record.  This is done on purpose so that all interfaces for all entities have
unique method signatures for use in the master interface (discussed in the next section).  If you do not like/want this style of functionality, you can skip 
all three of these BaseDao interfaces completely and manually create all the CRUD calls in each entity IDao (as well as whatever other functionality you need).
The only thing to remember is that you must make each method signature unique 
(so instead of just ```T Get(T item);``` you have ```Customer GetCustomer(int customerId);```))

```
T Get(T item);
void Insert(T item);
int Update(T item);
int Delete(T item);
```

###### IBaseGetAllDao

IBaseGetAllDao inherits from IBaseDao, but additionally specifies a **GetAll** call.  This call also requires the parameter similar to the above mentioned situation.
```
IList<T> GetAll(T item);
```

###### IBasePagedDao
IBasePagedDao also inherits from IBaseDao, and additionally specifies a **GetCount** and **GetPaged** calls.  More of the same with the required parameters, 
however these functions are used to get the upper boundary of how many of an entity you have, and then getting a particular subset of them; useful for user interface
based queries when trying to optimize data being queried and sent to your front end.

```
IList<T> GetPaged(T item, int skip, int take);
int GetCount(T item);
```

##### I[ProjectName]DataAccess
This is the master interface for your DAL implementation.  This shouldn't have any methods manually specified in it, this should be an interface of interfaces.
In here you'll inherit all the IDao's you created for each of your entities before.  In some cases you might have some functionality that doesn't persist to a 
particular entity and don't feel like it fits in an any entity's DAO, in those cases you would put the method signature here.


#### [ProjectName].DataAccess.[ImplementationName]

So technically, you can implement your DAL however you want to, so long as your main point of instantiation inherits/implements the IDataAccess interface defined above.
Currently my approach has been to create a DAO for each IDao specified, that way it's easier to find things in your source code.  
```internal class CustomerDao : ICustomerDao``` will set you up so that all the required CRUD calls and other methods you specified will be added and typed to 
the correct entity type.  I recommend using internal on the specific DAO's as these are separate for the convenience of the developer and separation of logic, but
shouldn't be accessible outside of the project, only the class implementing IDataAccess should be created and accessible outside of this project.

Depending on your actual implementation, you might have a bunch of other stuff in this project to support how the DAOs actually interact with the data storage
you're using.  However the final piece to mention here is the main IDataAccess implementation.  Generally I name this class [ProjectName]DataAccess, but it doesn't
matter.  If you created the separate DAOs for each entity, then in here you will have to instantiate each of those DAOs and just pass thru each method signature/call
into each specific DAO.


```
public class ExampleDataAccess : BaseDataAccessInt, IExampleDataAccess
{
    private readonly ICustomerDao _customerDao = new CustomerDao();
        
    public Customer Get(Customer item){
        return _customerDao.Get(item);
    }
}
```


## Running the tests

The library has 21 unit tests currently.  I tried to cover everything possible.  They are created with an XUnit test project, as well as an Example project.


## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess/tags). 

## Authors

* **G. Gordon Nasseri** - *Initial work* - [ProphetManX](https://github.com/ProphetManX)

See also the list of [contributors](https://github.com/ProphetManX/ProphetsWay.BaseDataAccess/graphs/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details


