# Project Setup 

### Vagrant

NOTE: MUST HAVE VIRTUALBOX INSTALLED

Run `vagrant up` to start the Vagrant VM

### MinIO

1. Navigate to http://localhost:9000 to access MinIO. This is what you'll be using for asset storage for development. The credentials are: `access_key` and `secret_key`.
2. Once logged in create a bucket named `c9s-assetstore` and inside there create a folder named `robot`.
3. Inside the robot folder add a `.glb` file and any image (the name of the files don't matter)

### Vault

1. Navigate to http://localhost:8200/ui to access Vault. This is what you'll be using for secrets management for development. The credential is: "token"
2. Once logged in you should see a folder named `secret/` in the secrets tab. Click the elipses (...) and delete the folder.
3. Click on "Enable New Engine" then select KV then press next
4. In the path field put "secret" and put the version as 1
5. After you've enabled it add a dummy secret (The values don't matter)

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