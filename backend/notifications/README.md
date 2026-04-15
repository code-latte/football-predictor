# FootballCatch.Notifications

Microservice responsible for sending **push notifications and emails**.

---

## Emitted events
- `notification.sent.v1`


## Consumed events
- `match.kickoff.v1`
- `prediction.locked.v1`
- `scoring.userScoreUpdated.v1`


---

## Infrastructure

- Own database (PostgreSQL).
- REST endpoints exposed according to responsibilities.
- Endpoint `/metrics` for Prometheus.
