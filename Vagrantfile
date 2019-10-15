# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
    config.vm.box = "centos/7"
    config.vm.network "forwarded_port", guest: 9000, host: 9000, host_ip: "127.0.0.1"
    config.vm.network "forwarded_port", guest: 8200, host: 8200, host_ip: "127.0.0.1"
    config.vm.network "private_network", ip: "192.168.50.50"
    config.vm.synced_folder ".", "/vagrant", type: "rsync"

    config.vm.provision "docker" do |d|
        d.run "vault", args: "-p 8200:8200 --cap-add=IPC_LOCK -e 'VAULT_DEV_ROOT_TOKEN_ID=token'"
        d.run "mysql", args: "-e MYSQL_ROOT_PASSWORD=devpass"
        d.run "minio/minio", 
          args: "-p 9000:9000 -e MINIO_ACCESS_KEY=access_key -e MINIO_SECRET_KEY=secret_key -v /mnt/data:/data", 
          cmd: "server /data"
    end
end
