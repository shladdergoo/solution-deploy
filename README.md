solution-deploy

[![Build status](https://hcc-devops.visualstudio.com/CI/_apis/build/status/solutiondeploy-ci)](https://hcc-devops.visualstudio.com/CI/_build/latest?definitionId=6)

Allows for multiple independent microservices to be logically grouped as a single solution and deployed as a single operation.

Imagine:
```
Commerce Platform 1.0.0
| - Billing Service 1.0.8
| - Customer Service 2.1.0
| - Payment Service 1.0.23
Commerce Platform 2.0.0
| - Billing Service 2.0.0
| - Customer Service 2.1.0
| - Payment Service 1.0.34
```

This is captured in the 
[manifest file](https://github.com/shladdergoo/solution-deploy/blob/master/tst/SolutionDeploy.Test.Integration/testdata/manifest.json)

solution-deploy calls the Azure DevOps REST APIs to drive the deployment of the required services, at the required version into the target environment.

Commands:
- Deploy

Options:  
-productName  
The name of the product to be deployed  
-environment  
The name of the deployment environment  
-v | --version  
The version number of the product to be deployed  
-b | --branch  
The name of the code branch to be used for deployment
-p | --allow-partial  
Consider partially succeeded releases to pre-req environment  
-w | --what-if  
Run in 'what-if' mode. No releases will be deployed

Uses OAuth in place of PAT authorisation - see https://github.com/shladdergoo/authorization-service for details of the web service that carries out the OAuth handshake and generates the tokens.