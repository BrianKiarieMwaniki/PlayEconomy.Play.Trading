# PlayEconomy.Play.Trading

Trading microservice for play economy.

## Build docker image

```powershell
$version="1.0.1"
$env:GH_OWNER="PlayEcomony-Microservices"
$env:GH_PAT="[PAT HERE]"
$acrName="playeconomybkm"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$acrName.azurecr.io/play.trading:$version" . 
```

## Run the docker image

```powershell
$cosmosDbConnStr="[CONN STRING HERE]"
$serviceBusConnString="[CONN STRING HERE]"
docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__ConnectionString=$cosmosDbConnStr -e ServiceBusSettings__ConnectionString=$serviceBusConnString -e ServiceSettings__MessageBroker="SERVICEBUS" -e QueueSettings__GrantItemsQueueAddress="queue:inventory-grant-items" -e QueueSettings__DebitGilQueueAddress="queue:identity-debit-gil" -e QueueSettings__SubtractItemsQueueAddress="queue:inventory-subtract-items" play.trading:$version
```

## Publish Docker Image to Azure Container Registry

```powershell
az acr login --name $acrName
docker push "$acrName.azurecr.io/play.trading:$version"
```

## Create the Kubernetes namespace

```powershell
$namespace="trading"
kubectl create namespace $namespace
```

## Create the Kubernetes secrets

```powershell
kubectl create secret generic trading-secrets --from-literal=cosmosdb-connectionstring=$cosmosDbConnStr --from-literal=servicebus-connectionstring=$serviceBusConnString -n $namespace
```
