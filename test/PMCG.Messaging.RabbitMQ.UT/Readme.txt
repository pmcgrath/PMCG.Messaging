
# Only works in bash
data='{ "routing_key": "", "properties": { "type": "RateSetActivated" }, "payload": "{ \"Id\": \"ad842b45-e42a-4ef6-882b-36f5dccadf58\" }", "payload_encoding": "string" }'
curl -i -u 'guest:guest' -H 'Content-Type:application/json' -d "$data" -X POST http://127.0.0.1:15672/api/exchanges/dev/pcs.offerevents/publish

