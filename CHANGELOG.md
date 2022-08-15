# v2.2.0
### Added Generic CRUD Calls
Added the ability to call ```Insert```, ```Update```, and ```Delete``` generically, similar to how ```Get<\T>(int id)``` works already.

# v2.1.3, v2.1.4
### Minor pipeline fixes
No functional changes, just updating how the pipeline triggers and the readme file.

# v2.1.2
### Updated to support .net 6.0
Updated the pipeline template to be more robust and reusable across many of my other projects.  Also updated the build
targets to support .net 6.0

# v2.1.0
### Rolled up ```IBaseDataAccess<TIdType>```/```BaseDataAccess<TIdType>``` into their base classes
Updated the root base interface/class to include the generic ```Get``` method, because it's possible the user would build
a database where most records have an ```int``` primary key, but on one large transaction table you'd prefer to use ```long```
and yet in another you'd want to use ```Guid``` so you can share the key across contexts.  The original implementation
only allowed one type of primary key.  Tagged the old methods as Obsolete, but using them will give you a warning, suggestion
includes the note to simply remove the generic assignment.


# v2.0.0
### Updated to support .net 5.0
Updated a few things, unfortunately it removed a little bit of functionality, so it counts as a major update, 
even tho it's really quite a minor update.
- Removed target frameworks that are not longer supported by Microsoft (netcoreapp2.0, netcoreapp2.2, netcoreapp3.0).
- Added target framework for .Net 5.0.
- Added an icon for the package.
- Updated reference to ProphetsWay.Example 


# v1.1.1
Added support for .Net Framework 4.8 explicitly and updated the changelog to include changes from v1.1.0.
Updated ```IBaseDataAccessInt``` and ```IBaseDataAccessLong``` to implement the new ```IBaseDataAccess<T>```
created in version v1.1.0 to cut down on duplicate code.

Updated ```BaseDataAccessInt``` and ```BaseDataAccessLong``` to implement the new ```BaseDataAccess<T>``` 
created in version v1.1.0 to cut down on duplicate code.

Removed the Example projects and added a submodule pointing to [ProphetsWay.Example](https://github.com/ProphetManX/ProphetsWay.Example).
ProphetsWay.Example references a NuGet reference to this project, albeit a slightly older version.



# v1.1.0
### New Interfaces ```IBaseIdEntity<T>``` and ```IBaseDataAccess<T>```
For added functionality/flexibility, I added some code to specify the ID property of your entities, as well as its type.
Then was able to refactor the BaseDataAccess classes to make use of these new features.

Old classes are marked as Obsolete, but are still usable, all changes are backwards compatible.

##### ```IBaseIdEntity<T>```
Created a new optional interface, IBaseIdEntity, which inherits from IBaseEntity for backwards compatibility
but this new interface will specify a property "Id" that must exist on your entities, and the type of the Id is 
specified by the generic passed in.  In general its likely to be either int, long, or a Guid.

##### ```IBaseDataAccess<T>```
Created a new interface to replace the two older options ```IBaseDataAccessInt``` and ```IBaseDataAccessLong```.
Now supports ```Guid``` id types.

##### ```BaseDataAccess<TIdType>```
Created a new base abstract class to replace the two older options ```BaseDataAccessInt``` and ```BaseDataAccessLong```.
Now supports ```Guid``` id types.



# v1.0.0
### Initial proper release.  
Contains all the interfaces and a single abstract base class for implementing a decoupled Data Access Layer (DAL) for your software solution.  See the 
readme for more information and check out the Example project for a working solution to reference.