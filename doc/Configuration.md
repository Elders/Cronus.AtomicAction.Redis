#### `Cronus:AtomicAction:Redis:ConnectionString` >> *string | Required: No*
Configures the connection string where Redis is located. This is optional and if not set, the ConnectionName property will be used to get the connection string from ConnectionStrings configuration.

---

#### `Cronus:AtomicAction:Redis:ConnectionName` >> *string | Required: No*
The name of the connection string to use. This is optional and if not set, defaults to "redis".

---

#### `Cronus:AtomicAction:Redis:LockTtl` >> *TimeSpan | Required: No | Default: 00:00:01.000*

---

#### `Cronus:AtomicAction:Redis:ShorTtl` >> *TimeSpan | Required: No | Default: 00:00:01.000*

---

#### `Cronus:AtomicAction:Redis:LongTtl` >> *TimeSpan | Required: No | Default: 00:00:05.000*

---

#### `Cronus:AtomicAction:Redis:LockRetryCount` >> *int | Required: No | Default: 3*

---

#### `Cronus:AtomicAction:Redis:LockRetryDelay` >> *TimeSpan | Required: No | Default: 00:00:00.100*

---

#### `Cronus:AtomicAction:Redis:ClockDriveFactor` >> *double | Required: No | Default: 0.01*

---
