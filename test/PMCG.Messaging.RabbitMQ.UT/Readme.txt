


# Works in bash
data='{ "routing_key": "", "properties": { "type": "RateSetActivated" }, "payload": "{ \"Id\": \"ad842b45-e42a-4ef6-882b-36f5dccadf58\" }", "payload_encoding": "string" }'
curl -i -u 'guest:guest' -H 'Content-Type:application/json' -d "$data" -X POST http://127.0.0.1:15672/api/exchanges/dev/pcs.offerevents/publish



# Powershell - horrific escaping
$requestTemplate = '{ "routing_key": "", "properties": { "type": "RateSetActivated" }, "payload": "[[payload]]", "payload_encoding": "string" }';
$payloadTemplate = '{ "Id": "[[Id]]" }';

$payloadData = $payloadTemplate.Replace('[[Id]]', [Guid]::NewGuid().ToString());
$payloadData = $payloadData.Replace('"', '\\"');		# Json witin json escaping

$requestData = $requestTemplate.Replace('[[payload]]', $payloadData);
$requestData = $requestData.Replace('"', '\"');		# Powershell issue passing to curl - need to escape the quotes again

# Can use fiddler with --proxy 127.0.0.1:8888 param
curl -i -u 'guest:guest' -H 'Content-Type:application/json' -d $requestData -X POST http://127.0.0.1:15672/api/exchanges/dev/pcs.offerevents/publish



# Powershell - horrific escaping - Run in loop
$requestTemplate = '{ "routing_key": "", "properties": { "type": "RateSetActivated" }, "payload": "[[payload]]", "payload_encoding": "string" }';
$payloadTemplate = '{ "Id": "[[Id]]" }';

do
{
	$id = [Guid]::NewGuid().ToString();
	$payloadData = $payloadTemplate.Replace('[[Id]]', $id);
	$payloadData = $payloadData.Replace('"', '\\"');		# Json witin json escaping

	$requestData = $requestTemplate.Replace('[[payload]]', $payloadData);
	$requestData = $requestData.Replace('"', '\"');		# Powershell issue passing to curl - need to escape the quotes again

	write-host "About to publish for message with id " + $id;
	curl -i -u 'guest:guest' -H 'Content-Type:application/json' -d $requestData -X POST http://127.0.0.1:15672/api/exchanges/dev/pcs.offerevents/publish | out-null;
	write-host;
} while((read-host 'X to exit') -ne 'X')



# Powershell - using a dictionary - horrific escaping - Run in loop
$requestTemplate = '{ "routing_key": "", "properties": { "type": "TheEvent", "message_id": "[[messageid]]" }, "payload": "[[payload]]", "payload_encoding": "string" }';
$payloadTemplate = '{ "Id": "[[Id]]" }';

do
{
	$payload = @{};
	$payload.Id =[Guid]::NewGuid().ToString();

	$payloadData = convertto-json $payload;
	$payloadData = $payloadData.Replace('"', '\\"');		# Json witin json escaping

	$requestData = $requestTemplate.Replace('[[messageid]]', $payload.Id);
	$requestData = $requestData.Replace('[[payload]]', $payloadData);
	$requestData = $requestData.Replace('"', '\"');		# Powershell issue passing to curl - need to escape the quotes again

	write-host "About to publish for message with id " + $id;
	curl -i -u 'guest:guest' -H 'Content-Type:application/json' -d $requestData -X POST http://127.0.0.1:15672/api/exchanges/dev/pcs.offerevents/publish | out-null;
	write-host;
} while((read-host 'X to exit') -ne 'X')







