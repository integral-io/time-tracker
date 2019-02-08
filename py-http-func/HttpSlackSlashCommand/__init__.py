import logging
import azure.functions as func
import os
from azure.servicebus import ServiceBusClient
from azure.servicebus import Message

def main(req: func.HttpRequest) -> func.HttpResponse:
    req_body_str = req.get_body().decode("utf-8")
    
    logging.info('Python HTTP trigger function processed a request: ' + req_body_str)

    # todo: pull this from env var with `os.environ['SB_CONN_STR']`
    sb_connectStr = os.environ['SB_CONN_STR']

    if not sb_connectStr: return func.HttpResponse(body='Service Bus connection string not set', status_code=500)
    
    sb_client = ServiceBusClient.from_connection_string(sb_connectStr)
    queue_client = sb_client.get_queue("slack-slash-commands")

    message = Message(req_body_str)
    queue_client.send(message)

    return func.HttpResponse(body='', status_code=200)
    
