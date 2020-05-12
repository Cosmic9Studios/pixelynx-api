# -*- mode: ruby -*-
# vi: set ft=ruby :

$enable_serial_logging = false

Vagrant.configure("2") do |config|
    config.vm.box = "centos/7"
    config.vm.network "forwarded_port", guest: 9000, host: 9000, host_ip: "127.0.0.1"
    config.vm.network "forwarded_port", guest: 8200, host: 8200, host_ip: "127.0.0.1"
    config.vm.network "forwarded_port", guest: 5432, host: 5432, host_ip: "127.0.0.1"
    config.vm.network "forwarded_port", guest: 1025, host: 1025, host_ip: "127.0.0.1"
    config.vm.network "forwarded_port", guest: 1080, host: 1080, host_ip: "127.0.0.1"
    config.vm.network "private_network", ip: "192.168.50.50"
    config.vm.synced_folder ".", "/vagrant", type: "rsync"

    config.vm.provider "virtualbox" do |v|
        v.customize ["modifyvm", :id, "--cpuexecutioncap", "10"]
    end

    config.vm.provision "docker" do |d|
        d.build_image "/vagrant/postgres", args: "-t local/postgres"
        d.run "vault", 
            image: "vault:1.3.2",
            args: "-p 8200:8200 -v /vagrant/vault-entrypoint.sh:/scripts/entrypoint.sh --cap-add=IPC_LOCK -e 'VAULT_DEV_ROOT_TOKEN_ID=token' -e 'VAULT_ADDR=http://0.0.0.0:8200' -e 'VAULT_DEV_LISTEN_ADDRESS=0.0.0.0:8200' --entrypoint /scripts/entrypoint.sh"
        d.run "postgres",
            image: "local/postgres",
            args: "-p 5432:5432 -e POSTGRES_PASSWORD=devpass -e POSTGRES_USER=superuser -e POSTGRES_DB=postgres",
            cmd:  "postgres -c log_statement=all -c log_destination=stderr"
        d.run "minio/minio", 
            args: "-p 9000:9000 -e MINIO_ACCESS_KEY=access_key -e MINIO_SECRET_KEY=secret_key -v /mnt/data:/data", 
            cmd: "server /data"
        d.run "maildev",
            image: "djfarrelly/maildev",
            args: "-p 1080:80 -p 1025:25"
    end
end
