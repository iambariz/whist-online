#!/bin/bash
sudo docker run -d --name whistdb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=whistdb -p 5432:5432 --restart always postgres:16
