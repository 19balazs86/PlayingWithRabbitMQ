SET resGroupName = test-ResGroup
call az group create --name %resGroupName% --location "North Europe"
call az servicebus namespace create --name "test-ServBus" --resource-group %resGroupName% --location "North Europe" --sku "Standard"