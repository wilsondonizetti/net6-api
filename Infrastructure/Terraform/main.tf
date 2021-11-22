## https://registry.terraform.io/providers/cyrilgdn/rabbitmq/latest/docs

# Create a virtual host
##### VIRTUAL HOST #####
resource "rabbitmq_vhost" "test" {
  name = "test"
}

resource "rabbitmq_permissions" "user" {
  user  = var.username
  vhost = "${rabbitmq_vhost.test.name}"

  permissions {
    configure = ".*"
    write     = ".*"
    read      = ".*"
  }
  depends_on = [rabbitmq_vhost.test]
}

##### EXCHANGE #####
resource "rabbitmq_exchange" "test" {
  name  = "xch.test"
  vhost = "${rabbitmq_vhost.test.name}"

  settings {
    type        = "fanout"
    durable     = false
    auto_delete = true
  }
  depends_on = [rabbitmq_permissions.user]
}

##### QUEUE #####
resource "rabbitmq_queue" "test" {
  name  = "ha.queue.test"
  vhost = "${rabbitmq_vhost.test.name}"

  settings {
    durable     = true
    auto_delete = false
    arguments_json = "${var.arguments_queue_test}"
  }
  depends_on = [rabbitmq_exchange.test]
}

variable "arguments_queue_test" {
  default = <<EOF
{
  "x-message-ttl": 5000,
  "x-queue-type" : "classic",
  "x-dead-letter-exchange": "xch.dlq.test",
  "x-dead-letter-routing-key" : "rk.queue.dlq.test"
}
EOF
}

resource "rabbitmq_binding" "binding_test" {
  source           = rabbitmq_exchange.test.name
  vhost     	   = "${rabbitmq_vhost.test.name}"
  destination      = rabbitmq_queue.test.name
  destination_type = "queue"

  depends_on = [rabbitmq_exchange.test, rabbitmq_queue.test]
}

##### Retry #####
resource "rabbitmq_exchange" "test_retry" {
  name  = "xch.retry.test"
  vhost = "${rabbitmq_vhost.test.name}"

  settings {
    type        = "direct"
    durable     = true
    auto_delete = false
  }
  depends_on = [rabbitmq_permissions.user]
}

resource "rabbitmq_queue" "test_retry" {
  name  = "ha.queue.retry.test"
  vhost = "${rabbitmq_vhost.test.name}"

  settings {
    durable         = true
    auto_delete     = false
    arguments_json  = var.arguments_queue_test_retry
  }
  depends_on = [rabbitmq_exchange.test_retry]
}

variable "arguments_queue_test_retry" {
  default = <<EOF
{
  "x-queue-type" : "classic",
  "x-dead-letter-exchange": "xch.retry.test",
  "x-dead-letter-routing-key" : "rk.queue.retry.test",
  "x-message-ttl" : 300000
}
EOF
}

resource "rabbitmq_binding" "test_retry" {
  source           = rabbitmq_exchange.test_retry.name
  vhost     	   = "${rabbitmq_vhost.test.name}"
  destination      = rabbitmq_queue.test.name
  destination_type = "queue"
  routing_key      = "rk.queue.test"

  depends_on = [rabbitmq_exchange.test_retry, rabbitmq_queue.test]
}

##### DLQ #####
resource "rabbitmq_exchange" "test_dlq" {
  name  = "xch.dlq.test"
  vhost = "${rabbitmq_vhost.test.name}"

  settings {
    type        = "direct"
    durable     = true
    auto_delete = false
  }
  depends_on = [rabbitmq_permissions.user]
}

resource "rabbitmq_queue" "test_dlq" {
  name  = "ha.queue.dlq.test"
  vhost = "${rabbitmq_vhost.test.name}"

  settings {
    durable         = true
    auto_delete     = false
    arguments_json  = var.queue_dlq_test_arguments
  }
  depends_on = [rabbitmq_exchange.test_dlq]
}

variable "queue_dlq_test_arguments" {
  default = <<EOF
{
  "x-queue-type" : "classic"
}
EOF
}

resource "rabbitmq_binding" "test_dlq" {
  source           = rabbitmq_exchange.test_dlq.name
  vhost     	   = "${rabbitmq_vhost.test.name}"
  destination      = rabbitmq_queue.test_dlq.name
  destination_type = "queue"
  routing_key      = "rk.queue.dlq.test"

  depends_on = [rabbitmq_exchange.test_dlq, rabbitmq_queue.test_dlq]
}