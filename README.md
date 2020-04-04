# Project Setup 

### Dev Certs 

Run `cd Pixelynx.Api`
Run `dotnet dev-certs https --clean` 
Run `dotnet dev-certs https -ep "localhost.pfx" -p 1234 --trust`

to enable https for local development

### Webhooks

If you are testing stripe webhooks, you have 2 options

Option 1: Stripe's Cli
Option 2: UltraHook -- http://www.ultrahook.com/ (If you decide to use UltraHook, you will need to let the Stripe Admin know so they can add your webhook address)

##### UltraHook

Once installed run `ultrahook stripe https://localhost:5000/stripe` to route stripe requests to the webhook

### Vagrant

NOTE: MUST HAVE VIRTUALBOX INSTALLED

Run `vagrant up` to start the Vagrant VM

### MinIO

1. Navigate to http://localhost:9000 to access MinIO. This is what you'll be using for asset storage for development. The credentials are: `access_key` and `secret_key`.
2. Once logged in create a bucket named `c9s-assetstore` and inside there create a folder named `robot`.
3. Inside the robot folder add a `.glb` file and any image (the name of the files don't matter)

### Vault

Navigate to http://localhost:8200/ui to access Vault. This is what you'll be using for secrets management for development. The credential is: "token"
All the default data is set for you via the Vagrantfile so you just need to run the code =)

### Running the Application

1. Add the ASPNETCORE_ENVIRONMENT variable and set the value to `Development`
1. Run the application either through your IDE or by running `dotnet run` in the parent directory
2. Navigate to http://localhost:5000/graphiql and run the following graphql query

```graphql
query {
    assets {
        name
        uri
        thumbnailUri
    }
}
```

3. In the "data" section of the result you should see the name: robot and you should have temporary links to your .glb file and the image you added robot folder.


### Migrations 

Whenever you make a change to a db entity you need to make a migration. This migration will be used to update the database to reflect the changes made to the entity. 
In order to create migrations you must first run: `dotnet tool install --global dotnet-ef` to install the entity framework cmd line tools.

To create a migration cd into the `Pixelynx.Data` folder and run 

`pwsh addMigration.ps1 ${MigrationName}` OR `powershell -ExecutionPolicy Bypass -File addMigration.ps1 ${MigrationName}` -- Replace `MigrationName` with the name of your migration Ex: `pwsh addMigration.ps1 AddedGroupTable`

Migrations will automatically be applied once you run the code, but if you want to apply them before then run the `updateDatabase.ps1` script.