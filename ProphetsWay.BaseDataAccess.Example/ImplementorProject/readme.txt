Everything in this folder  "ImplementorProject" should realistically be implemented in a separate project from your DataAccess project

YourProject.DataAccess			<--		Entity models and IDaos, as well as the IDataAccess master interface
YourProject.DataAccess.Impl		<--		The actual implementation of your Data Access Layer.  
										This is what hits the database or whatever you're using to store information. 
										This in theory would have a separate Dao for each IDao you specified, but it is not required to be constructed that way.
										The only requirement is to implement the IDataAccess master interface, 
										which utilizes your custom defined entity models and specific accessor methods you defined via custom interfaces