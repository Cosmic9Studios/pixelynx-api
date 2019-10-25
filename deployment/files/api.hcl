job "pixelynx-api" {
    datacenters = [ "dc1" ]

    group "pixelynx-api" {

        count = 1

        update {
            max_parallel = 2
            min_healthy_time = "30s"
            healthy_deadline = "5m"
            auto_revert = false
        }

        reschedule {
            delay          = "30s"
            delay_function = "exponential"
            max_delay      = "120s"
            unlimited      = true
        }

        task "run" {
            driver = "docker"

            config {
                image = "https://docker.pkg.github.com/phenry20/pixelynx-api/pixelynx-api:${version}"
                auth {
                    username = "${docker_user}"
                    password = "${docker_pass}"
                }

                port_map {
                    http = 5000
                }
            }

            resources {
                network {
                    port "http" {}
                }
            }

            env {
                ASPNETCORE_ENVIRONMENT = "Production"
            }

            service {
                name = "pixelynx-api"
                tags = ["urlprefix-/api strip=/api"]
                port = "http"

                check {
                    type     = "http"
                    port     = "http"
                    protocol = "http"
                    tls_skip_verify = true
                    interval = "10s"
                    path = "/graphiql"
                    timeout = "5s"
                }
            }
        }
    }
}