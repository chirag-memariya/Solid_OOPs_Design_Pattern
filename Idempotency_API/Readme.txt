- in software and tech idempotency typically refers to the idea that you can perform an operation multiple times without triggering any side effects more than once.

- we need idempotency in put and post only(why ? get only return and delete delete the data so..)

#problem
1) client send req - server - database - server - operation done in database - but server fails to send response to client 
2) client get error or no reply so it tries to send req again , then without idempotent server proceed this req and 
   send data to database for operation so it again do the same thing.
   ==> using key , it knows that we done that so simply send response to the client ok .


==> solution is x-idempotency-id = guid , as request header
-> we will keep track of this key from client to server

1) client send this key with req
2) server saved it or store it 
3) if client send same req again with same key then server knows that we  have this key so we not need to proceed this req bcz its already done and just send ok response.




==> we should not store this key to appliaction memory , either store it in database, or making use of redis cache