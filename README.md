# PlayEconomy.Play.Trading

Trading microservice for play economy.

## Build docker image

```powershell
$env:GH_OWNER="PlayEcomony-Microservices"
$env:GH_PAT="[PAT HERE]"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t play.trading:$version . 
```

## Run the docker image

```powershell
$grantItemsQueueAddress="queue:inventory-grant-items"
$debitGilQueueAddress="queue:identity-debit-gil"
$subractItemsAddress="queue:inventory-subtract-items"
docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq -e QueueSettings__GrantItemsQueueAddress=$grantItemsQueueAddress -e QueueSettings__DebitGilQueueAddress=$debitGilQueueAddress -e QueueSettings__SubtractItemsQueueAddress=$subractItemsAddress --network playinfra_default play.trading:$version
```
