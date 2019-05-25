job "assetstore" {
    datacenters = [ "dc1" ]

    group "assetstore" {

        count = 1

        update {
            max_parallel = 2
            min_healthy_time = "30s"
            healthy_deadline = "5m"

            # Enable automatically reverting to the last stable job on a failed
            # deployment.
            auto_revert = true
        }

        reschedule {
            attempts       = 15
            interval       = "1h"
            delay          = "30s"
            delay_function = "exponential"
            max_delay      = "120s"
            unlimited      = false
        }

        task "run" {
            driver = "docker"

            config {
                image = "https://registry.gitlab.com/cosmic9studios/assetstore/api:${version}"
                auth {
                    username = "${gitlabUser}"
                    password = "${gitlabPass}"
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

            service {
                name = "assetstore-api"
                tags = ["urlprefix-/ proto=https tlsskipverify=true"]

                port = "http"

                check {
                    type     = "http"
                    port     = "http"
                    protocol = "https"
                    tls_skip_verify = true
                    interval = "10s"
                    path = "/health"
                    timeout = "5s"
                }
            }
        }
    }
}