## Parent PRD

`issues/prd.md`

## What to build

Implement the core Review insertion engine. This ensures tenants holding a `StatusId = 4` (Completed) transaction record are allowed to input a numerical star rating and textual comment. This entails defining the baseline Repository, Service layer validation logic, and routing endpoints required to write records to persistence.

## Acceptance criteria

- [ ] Add `IReviewRepository` and EF implementation with `AddReviewAsync` and standard persistence operations.
- [ ] Build `ReviewService` incorporating dynamic verification check `HasCompletedBookingAsync`.
- [ ] Establish dynamic POST endpoint `Review/Submit` enforcing session authorization and anti-forgery safeguards.
- [ ] Introduce interactive UI button triggering review workflow visible only for Completed bookings in user Dashboards.
- [ ] Ensure system prevents review persistence if user does not own a correlated completed booking.

## Blocked by

- None - can start immediately

## User stories addressed

- User story 1
- User story 4
- User story 9
