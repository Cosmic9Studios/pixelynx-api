# Project Setup 

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