# Fargate Blog

This is a blank project for CDK development with C#.

The `cdk.json` file tells the CDK Toolkit how to execute your app.

It uses the [.NET CLI](https://docs.microsoft.com/dotnet/articles/core/) to compile and execute your project.

## Installation instructions

download .NET+

download nodejs
https://nodejs.org/
* `dotnet tool install -g aws.deploy.tools`
* `npm install -g aws-cdk`

from root folder
* `cdk bootstrap`
* `cdk synth`
* `cdk deploy --all`
	
login to aws ecr
* `aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin [account id].dkr.ecr.[region].amazonaws.com`
	
build container and push
* `cd src`
* `docker build -t fargate-blog-encoder .`
	
* `docker tag fargate-blog-encoder:latest  [account id].dkr.ecr.[region].amazonaws.com/fargate-blog-repository:latest`
	
* `docker push [account id].dkr.ecr.[region].amazonaws.com/fargate-blog-repository:latest`
	
* `cd ..`
