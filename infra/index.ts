import NomadJob from "@cosmic9studios/pulumi-nomad";
import * as pulumi from "@pulumi/pulumi";
import * as fs from "fs";

const config = new pulumi.Config();

const version = config.get("version") || "latest";
const gitlabUser = config.get("gitlabUser");
const gitlabPass = config.get("gitlabPass");

const nomadJob = new NomadJob("AssetStore",  { 
    address: `http://nomad.cosmic9studios.ca:4646`, 
    hclJob: fs.readFileSync(`${__dirname}/jobs/assetstore.hcl`, "utf-8"), 
    vars: {
        version,
        gitlabUser, 
        gitlabPass
    },
    retryCount: 20
});

