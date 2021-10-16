ACL

`consul acl bootstrap`
```
AccessorID:       dd613752-a04c-fd6f-5dee-b40d618d690c
SecretID:         b3a3b631-d655-fb23-253c-bdb559ebaf22
Description:      Bootstrap Token (Global Management)
Local:            false
Create Time:      2021-10-16 04:01:24.3220923 +0000 UTC
Policies:
   00000000-0000-0000-0000-000000000001 - global-management
```
export CONSUL_HTTP_TOKEN=b3a3b631-d655-fb23-253c-bdb559ebaf22
```
consul acl policy create -name "agent-token" -description "Agent Token Policy" -rules @agent-policy.hcl
```

# Notes

- Use Environment variable `CONSUL_HTTP_TOKEN`
```export CONSUL_HTTP_TOKEN=e9dc6ee0-39e7-8cf2-890f-9f31cf895e8c```

- Use EnvVar `CONSUL_HTTP_TOKEN_FILE` pointing to `/consul/tokens/http-token.txt`

# Agent Policy Token 

`consul acl token create -description "Agent Token" -policy-name "agent-token"`

```
AccessorID:       2f61a075-e0cc-6059-0d25-2a2a3a6fceb1
SecretID:         bcf865c3-2900-c7b9-d430-6b28fb63283e
Description:      Agent Token
Local:            false
Create Time:      2021-10-16 04:03:18.5629218 +0000 UTC
Policies:
   019ee74a-833e-4f73-30fa-d9f6fda4eaec - agent-token
```