#!/bin/bash

# Variables
base_url="http://172.16.0.122:32001/api/FaceDetection"
total_requests=20     # Total number of requests to perform for the benchmarking session

# Function to perform load testing with curl
perform_load_test() {
    i=1
    while [ $i -le $total_requests ]; do

        echo $i
        endpoint="$base_url?cameraId=$i"
        curl -s -o /dev/null -w "Camera ID: $i - HTTP Code: %{http_code}\n" $endpoint &
        
        i=$((i + 1))

        # Add a delay of 100 milliseconds between requests
        sleep 0.5
    done

    # Wait for all background processes to finish
    wait
}

# Perform load testing
perform_load_test
