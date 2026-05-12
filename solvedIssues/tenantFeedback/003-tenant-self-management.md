## Parent PRD

`issues/prd.md`

## What to build

Expand core API surfaces enabling localized mutation rights for reviewers. This enforces security check-backs validating that current caller IDs align perfectly with origin Author ID payloads before unlocking UPDATE or DESTROY routines for existing review records.

## Acceptance criteria

- [ ] Create secure `Review/Delete/{id}` routing verifying original owner claim before executing `_repo.Delete`.
- [ ] Deploy distinct visual prompt toggles (Edit/Delete) constrained to show ONLY when `CurrentUserId == AuthorId`.
- [ ] Formulate interactive edit state preserving original content in pre-populated fields.
- [ ] Guarantee cascade logic ensures corresponding database rows update instantly without affecting property listing integrity.

## Blocked by

- Blocked by `issues/001-submit-tenant-feedback.md`

## User stories addressed

- User story 5
- User story 6
