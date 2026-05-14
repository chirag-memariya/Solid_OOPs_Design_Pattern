kubectl run -i --tty load-generator --rm --image=curlimages/curl:latest --restart=Never -- /bin/sh


curl http://172.16.1.58:32001/api/FaceDetection/goHandler

curl -X GET http://172.16.1.58:32001/api/FaceDetection?cameraId=1

curl -X GET -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" http://172.16.1.58:32001/api/FaceDetection?cameraId=camera123


curl -s -o /dev/null -w "Camera ID: $camera_id - HTTP Code: %{http_code}\n" $endpoint 


base_url="http://172.16.1.246:32001/api/FaceDetection" && for camera_id in {1..20}; do endpoint="$base_url?cameraId=$camera_id"; response=$(curl -X GET -s -o /dev/null -w "Camera ID: $camera_id - HTTP Code: %{http_code}\n" $endpoint); echo "$response"; done


base_url="http://172.16.1.246:32001/api/FaceDetection"

while sleep 0.5; do camera_id=$((RANDOM % 2000 + 1)); endpoint="$base_url?cameraId=$camera_id"; response=$(curl -X GET -s -o /dev/null -w "Camera ID: $camera_id - HTTP Code: %{http_code}\n" $endpoint); echo "$response"; done

while sleep 0.001; do camera_id=$((RANDOM % 1000 + 1)); endpoint="$base_url?cameraId=$camera_id"; response=$(curl -X GET -s -o /dev/null -w "Camera ID: $camera_id - HTTP Code: %{http_code}\n" $endpoin
t); echo "$response"; done

test


"while sleep 0.05; do wget -q -O- http://php-apache; done"

<!-- Connecting to 192.168.27.127:80 (192.168.27.127:80)
wget: server returned error: HTTP/1.1 404 Not Found
~ $ curl -X GET -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" http://172.16.1.58:32001/api/FaceDetection?cameraId=camera123
Camera ID: 1 - HTTP Code: 000
~ $ curl -X GET -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" http://172.16.1.58:32001/api/FaceDetection?cameraId=1
^C
~ $ curl -X GET -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" http://172.16.1.246:32001/api/FaceDetection?cameraId=1
Camera ID: 1 - HTTP Code: 200
~ $ curl -X GET -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" http://172.16.1.246:32001/api/FaceDetection?cameraId=1
Camera ID: 1 - HTTP Code: 000
~ $ curl -X GET -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" http://172.16.1.246:32001/api/FaceDetection?cameraId=1
Camera ID: 1 - HTTP Code: 000
~ $ base_url="http://172.16.1.246:32001/api/FaceDetection"
~ $ 
~ $ for camera_id in {1..200}; do
>     endpoint="$base_url?cameraId=$camera_id"
>     response=$(curl -X GET -s -o /dev/null -w "Camera ID: $camera_id - HTTP Code: %{http_code}\n" $endpoint)
>     echo "$response"
> done
Camera ID: {1..200} - HTTP Code: 200
~ $ base_url="http://172.16.1.246:32001/api/FaceDetection" && for camera_id in {1..20}; do endpoint="$base_url?cameraId=$camera_id"; response=$(curl -X GET -s -o /dev/null -w "Camera ID: $camera_id - HTTP
 Code: %{http_code}\n" $endpoint); echo "$response"; done
Camera ID: {1..20} - HTTP Code: 200
~ $ base_url="http://172.16.1.246:32001/api/FaceDetection" && for camera_id in {1..2000}; do endpoint="$base_url?cameraId=$camera_id"; response=$(curl -X GET -s -o /dev/null -w "Camera ID: $camera_id - HT
TP Code: %{http_code}\n" $endpoint); echo "$response"; sleep 0.5; done
Camera ID: {1..2000} - HTTP Code: 200
~ $ exit
pod "load-generator" deleted -->

# metrix-server

kubectl get apiservices.apiregistration.k8s.io

# hpa service
kubectl autoscale deployment fd-service --cpu-percent=50 --min=1 --max=10

# test
# final 

base_url="http://172.16.1.246:32001/api/FaceDetection"

# get
i=0
while sleep 0.05; do
    camera_id=$i
    endpoint="$base_url?cameraId=$camera_id"
    response=$(curl -X GET -s -o /dev/null -w "Camera ID: $camera_id - HTTP Code: %{http_code}\n" "$endpoint")
    echo "$response"
    ((i++))
done

# delete
i=0
while sleep 0.05; do
    camera_id=$i
    endpoint="$base_url?cameraId=$camera_id"
    response=$(curl -X DELETE -s -o /dev/null -w "Camera ID: $camera_id - HTTP Code: %{http_code}\n" "$endpoint")
    echo "$response"
    
    # Check if i is equal to 88, and brea k the loop if true
    if [ "$i" -eq 88 ]; then
        break
    fi

    ((i++))
done



base_url="http://172.16.1.246:32001/api/FaceDetection"

i=0;
while sleep 0.05;do camera_id=$i; endpoint="$base_url?cameraId=$camera_id"; response=$(curl -X GET -s -o /dev/null -w "Camera ID: $camera_id - HTTP Code: %{http_code}\n" $endpoint); echo "$response";$i=$i+1; done

# all curl request
<!-- curl -X GET -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" http://172.16.1.246:32001/api/FaceDetection?cameraId=1 -->

curl -X 'GET' -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" 'http://172.16.1.246:32001/api/FaceDetection?cameraId=1' -H 'accept: */*'

curl -X 'DELETE' -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" 'http://172.16.1.246:32001/api/FaceDetection?cameraId=1' -H 'accept: */*'

curl -X 'GET' -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" 'http://172.16.1.246:32001/api/FaceDetection/health' -H 'accept: */*'

curl -X 'GET' -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" 'http://172.16.1.246:32001/api/FaceDetection/ready' -H 'accept: */*'

curl -X 'GET' -s -o /dev/null -w "Camera ID: 1 - HTTP Code: %{http_code}\n" 'http://172.16.1.246:32001/api/FaceDetection/goHandler' -H 'accept: */*'

# ok!


while sleep 0.01; do curl http://172.16.1.58:32001/api/FaceDetection/goHandler; done

curl http://172.16.1.58:32001/api/FaceDetection/goHandler

# curl image
kubectl run -i --tty load-generator --rm --image=curlimages/curl:latest --restart=Never -- /bin/sh


kubectl autoscale deployment fd-service --cpu-percent=50 --min=1 --max=10

kubectl describe hpa fd-service
<!-- Name:                                                  fd-service
Deployment pods:                                       1 current / 1 desired
Conditions:
  Type            Status  Reason                   Message
  ----            ------  ------                   -------
  AbleToScale     True    SucceededGetScale        the HPA controller was able to get the target's current scale
  ScalingActive   False   FailedGetResourceMetric  the HPA was unable to compute the replica count: failed to get cpu utilization: unable to get metrics for resource cpu: no metrics returned from resource metrics API
  ScalingLimited  True    TooFewReplicas           the desired replica count is less than the minimum replica count
Events:
  Type     Reason                        Age   From                       Message
  ----     ------                        ----  ----                       -------
  Warning  FailedGetResourceMetric       8s    horizontal-pod-autoscaler  failed to get cpu utilization: unable to get metrics for resource cpu: no metrics returned from resource metrics API
  Warning  FailedComputeMetricsReplicas  8s    horizontal-pod-autoscaler  invalid metrics (1 invalid out of 1), first error is: failed to get cpu resource metric value: failed to get cpu utilization: unable to get metrics for resource cpu: no metrics returned from resource metrics API -->

Conditions:
  <!-- Type            Status  Reason                   Message
  ----            ------  ------                   -------
  AbleToScale     True    SucceededGetScale        the HPA controller was able to get the target's current scale
  ScalingActive   False   FailedGetResourceMetric  the HPA was unable to compute the replica count: failed to get cpu utilization: unable to get metrics for resource cpu: no metrics returned from resource metrics API
  ScalingLimited  False   DesiredWithinRange       the desired count is within the acceptable range
Events:
  Type     Reason                        Age                From                       Message
  ----     ------                        ----               ----                       -------
  Normal   SuccessfulRescale             56s                horizontal-pod-autoscaler  New size: 3; reason: Current number of replicas below Spec.MinReplicas
  Warning  FailedGetResourceMetric       11s (x2 over 26s)  horizontal-pod-autoscaler  failed to get cpu utilization: unable to get metrics for resource cpu: no metrics returned from resource metrics API
  Warning  FailedComputeMetricsReplicas  11s (x2 over 26s)  horizontal-pod-autoscaler  invalid metrics (1 invalid out of 1), first error is: failed to get cpu resource metric value: failed to get cpu utilization: unable to get metrics for resource cpu: no metrics returned from resource metrics API -->

kubectl describe hpa php-apache
<!-- Name:                                                  php-apache
Deployment pods:                                       9 current / 9 desired
Conditions:
  Type            Status  Reason               Message
  ----            ------  ------               -------
  AbleToScale     True    ScaleDownStabilized  recent recommendations were higher than current one, applying the highest recent recommendation
  ScalingActive   True    ValidMetricFound     the HPA was able to successfully calculate a replica count from cpu resource utilization (percentage of request)
  ScalingLimited  False   DesiredWithinRange   the desired count is within the acceptable range
Events:
  Type     Reason                        Age                    From                       Message
  ----     ------                        ----                   ----                       -------
  Warning  FailedGetResourceMetric       2m34s (x2 over 2m49s)  horizontal-pod-autoscaler  failed to get cpu utilization: did not receive metrics for targeted pods (pods might be unready)
  Warning  FailedComputeMetricsReplicas  2m34s (x2 over 2m49s)  horizontal-pod-autoscaler  invalid metrics (1 invalid out of 1), first error is: failed to get cpu resource metric value: failed to get cpu utilization: did not receive metrics for targeted pods (pods might be unready)
  Normal   SuccessfulRescale             79s                    horizontal-pod-autoscaler  New size: 3; reason: cpu resource utilization (percentage of request) above target
  Normal   SuccessfulRescale             64s                    horizontal-pod-autoscaler  New size: 5; reason: cpu resource utilization (percentage of request) above target
  Normal   SuccessfulRescale             49s                    horizontal-pod-autoscaler  New size: 9; reason: cpu resource utilization (percentage of request) above target -->

  <!-- Max replicas:                                          10
Deployment pods:                                       9 current / 9 desired
Conditions:
  Type            Status  Reason               Message
  ----            ------  ------               -------
  AbleToScale     True    ScaleDownStabilized  recent recommendations were higher than current one, applying the highest recent recommendation
  ScalingActive   True    ValidMetricFound     the HPA was able to successfully calculate a replica count from cpu resource utilization (percentage of request)
  ScalingLimited  False   DesiredWithinRange   the desired count is within the acceptable range
Events:
  Type     Reason                        Age                   From                       Message
  ----     ------                        ----                  ----                       -------
  Warning  FailedGetResourceMetric       3m46s (x2 over 4m1s)  horizontal-pod-autoscaler  failed to get cpu utilization: did not receive metrics for targeted pods (pods might be unready)
  Warning  FailedComputeMetricsReplicas  3m46s (x2 over 4m1s)  horizontal-pod-autoscaler  invalid metrics (1 invalid out of 1), first error is: failed to get cpu resource metric value: failed to get cpu utilization: did            not           receive metrics for targeted pods (pods might be unready)
  Normal   SuccessfulRescale             2m31s                 horizontal-pod-autoscaler  New size: 3; reason: cpu resource utilization (percentage of request) above target -->

livenessProbe:
      failureThreshold: 8
      httpGet:
        host: 172.16.1.58
        path: /livez
        port: 6443
        scheme: HTTPS
      initialDelaySeconds: 10
      periodSeconds: 10
      successThreshold: 1
      timeoutSeconds: 15
    name: kube-apiserver
    readinessProbe:
      failureThreshold: 3
      httpGet:
        host: 172.16.1.58
        path: /readyz
        port: 6443
        scheme: HTTPS
      periodSeconds: 1
      successThreshold: 1
      timeoutSeconds: 15
    resources:
      requests:
        cpu: 250m
