# Contributing

## Integration Tests

The project includes integration tests using a fake Bitbucket API server.

### Run Integration Tests

```
cd tests/Integration
docker-compose up --exit-code-from pipe --build
```

If tests pass, command should exit with code 0.
