## https://registry.terraform.io/providers/cyrilgdn/rabbitmq/latest/docs

# Configure the RabbitMQ provider
provider "rabbitmq" {
  endpoint = var.provider_endpoint
  username = var.username
  password = var.password
}