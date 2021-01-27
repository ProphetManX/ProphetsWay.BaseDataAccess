# v2.0.0
Removed target frameworks that are not longer supported by Microsoft (netcoreapp2.0, netcoreapp2.2, netcoreapp3.0).
Added target framework for .Net 5.0.


# v1.1.1
Added support for .Net Framework 4.8 explicitly and updated the changelog to include changes from v1.1.0.
Updated ```IBaseDataAccessInt``` and ```IBaseDataAccessLong``` to implement the new ```IBaseDataAccess<T>```
created in version v1.1.0 to cut down on duplicate code.

Updated ```BaseDataAccessInt``` and ```BaseDataAccessLong``` to implement the new ```BaseDataAccess<T>``` 
created in version v1.1.0 to cut down on duplicate code.

Removed the Example projects and added a submodule pointing to [ProphetsWay.Example](https://github.com/ProphetManX/ProphetsWay.Example).
ProphetsWay.Example referenes a NuGet reference to this project, albeit a slightly older version.



# v1.1.0
### New Interfaces IBaseIdEntity< T > and IBaseDataAccess< T >
For added functionality/flexibility, I added some code to specify the ID property of your entities, as well as its type.
Then was able to refactor the BaseDataAccess classes to make use of these new features.

Old classes are marked as Obsolete, but are still usabled, all changes are backwards compatible.

##### IBaseIdEntity< T >
Created a new optional interface, IBaseIdEntity, which inherits from IBaseEntity for backwards compatibility
but this new interface will specify a property "Id" that must exist on your entities, and the type of the Id is 
specified by the generic passed in.  In general its likely to be either int, long, or a Guid.

##### IBaseDataAccess< T >
Created a new interface to replace the two older options ```IBaseDataAccessInt``` and ```IBaseDataAccessLong```.
Now supports ```Guid``` id types.

##### BaseDataAccess< TIdType >
Created a new base abstract class to replace the two older options ```BaseDataAccessInt``` and ```BaseDataAccessLong```.
Now supports ```Guid``` id types.



# v1.0.0
### Initial proper release.  
Contains all the interfaces and a single abstract base class for implementing a decoupled Data Access Layer (DAL) for your software solution.  See the 
readme for more information and check out the Example project for a working solution to reference.