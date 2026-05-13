![OAuth2.0 flow](image.png)
![Profile Scope OAuth2.0 flow](image-1.png)

### OAuth2.0 terminology
- scope
- policy


Question
- why do we have tow things why do we have to get an authorization code and then exchange that for the access token , why dont we just use the code or why dont we get the 
    access token immediately? why is there that extra step?(why we need to do this extra step, why we get a code instead of just getting the token right away?)
Answer
- 
![Secure Back channel](image-2.png)

![OAuthDebugger](image-3.png) - for testing auth 


![Calling back Responce back from auth server for getting authorization code](image-4.png)

![exchange code for an access token](image-5.png)
![Authorization server returns an access token](image-6.png)
![Use the access token while requesting to server](image-7.png)
![validation appproved by back channel](image-9.png)




### implicit flow(fronted only)
![aoth flows](image-10.png)
![alt text](image-8.png)

![Problem with OAuth 2.0 for authentication](image-11.png)
![what openid connect adds](image-12.png)
![openid profile for get access token and id token](image-14.png) - get user info with access token


![OAuth and OpenID Connect](image-15.png)
![PKCE flow OAuth2](image-16.png)