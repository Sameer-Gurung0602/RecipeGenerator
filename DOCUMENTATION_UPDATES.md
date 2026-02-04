# API Documentation Updates - Aligned with Test Files

## Summary
Updated `wwwroot/index.html` to accurately reflect the actual API behavior as verified by integration tests in `RecipeControllerTests.cs` and `FavouritesControllerTests.cs`.

---

## Changes Made

### 1. **GET /api/recipes/trending** ?
**What Changed:**
- Default `count` parameter changed from **3 to 4**
- Added note about automatic fetch count tracking
- Updated example response to show 4 recipes
- Added sorting behavior explanation
- Clarified that viewing a recipe increments its trending position

**Why:**
- Tests show default count is 4: `GetTrendingRecipes(int count = 4)`
- Tests verify descending sort by fetch count

---

### 2. **GET /api/recipes/ingredients** ?
**What Changed:**
- Updated response format to show objects with `ingredientId` and `ingredientName`
- Added note about alphabetical sorting
- Updated example to show all 10 test ingredients in sorted order
- Added usage notes about using ingredient IDs in match requests

**Before:**
```json
["Shrimp", "Lime", "Cilantro", ...]
```

**After:**
```json
[
  { "ingredientId": 51, "ingredientName": "Shrimp" },
  { "ingredientId": 52, "ingredientName": "Lime" },
  ...
]
```

**Why:**
- Tests expect `List<IngredientDto>` not `List<string>`
- Tests verify alphabetical sorting
- Service returns `.OrderBy(i => i.IngredientName)`

---

### 3. **POST /api/favourites/:userId/recipes** ?
**What Changed:**
- Added detailed validation rules for `recipeId`
- Documented error message for already favorited recipes: "already favourited"
- Added concurrency behavior notes
- Clarified all 400 Bad Request scenarios (zero, negative, null)
- Added example showing 404 response for duplicate favorites

**Why:**
- Tests validate zero and negative recipe IDs return 400
- Tests verify "already favourited" error message
- Tests check concurrent save attempts

---

### 4. **GET /api/recipes/:id** ?
**What Changed:**
- Added **automatic fetch count tracking** note
- Clarified this affects trending recipes ranking
- Added behavior notes section
- Documented default userId parameter

**Why:**
- Service calls `await IncrementFetchCount(id)` on every request
- Tests verify fetch count increments on view
- Critical for understanding trending recipes feature

---

### 5. **GET /api/favourites/:userId** ?
**What Changed:**
- Expanded response example to show multiple recipes
- Added complete property list
- Added behavior notes about eager loading
- Documented that `isFavourite` is always true
- Added note about empty array for users with no favorites

**Why:**
- Tests verify all navigation properties are loaded (Instructions, Ingredients, DietaryRestrictions)
- Tests check image URLs are present
- Tests verify multiple recipes can be returned

---

### 6. **DELETE /api/favourites/:userId/recipes/:recipeId** ?
**What Changed:**
- Added detailed behavior notes (idempotent, atomic, persistent)
- Added testing guide showing verification steps
- Clarified 404 applies to both missing recipe and not-favorited scenarios
- Added example 404 response format

**Why:**
- Tests verify deletion is persistent in database
- Tests check 404 for non-existent and non-favorited recipes
- Tests verify recipe no longer appears after deletion

---

## Key Documentation Improvements

### Consistency
- All examples now match actual test data
- Error codes aligned with controller responses
- Response formats match DTOs exactly

### Completeness
- All query parameters documented with defaults
- All error scenarios covered
- Behavior notes added for complex operations

### Developer Experience
- Added usage notes and examples
- Included testing guides for DELETE operations
- Clarified automatic behaviors (fetch count tracking)
- Documented edge cases (concurrency, null handling)

---

## Test Coverage Verification

| Endpoint | Tests Pass | Documentation Updated |
|----------|------------|----------------------|
| GET /api/recipes | ? 3 tests | ? Already accurate |
| GET /api/recipes/trending | ? 3 tests | ? Updated (default count) |
| GET /api/recipes/:id | ? 1 test | ? Updated (fetch tracking) |
| GET /api/recipes/ingredients | ? 6 tests | ? Updated (DTO format) |
| POST /api/recipes/match | ? 13 tests | ? Already accurate |
| GET /api/favourites/:userId | ? 13 tests | ? Updated (details) |
| POST /api/favourites/:userId/recipes | ? 12 tests | ? Updated (validation) |
| DELETE /api/favourites/:userId/recipes/:recipeId | ? 5 tests | ? Updated (behavior) |

**Total Tests:** 105 ? All Passing

---

## Files Modified

1. `wwwroot/index.html` - Complete API documentation
   - 6 sections updated
   - All examples verified against tests
   - Behavior notes added

---

## Validation

? All 105 integration tests pass
? Build successful
? Documentation matches controller implementations
? Error messages match actual responses
? Response formats match DTOs
? Default values documented correctly

---

## Developer Notes

### Fetch Count Tracking
The trending recipes feature works automatically:
1. User views recipe ? `GET /api/recipes/:id`
2. `IncrementFetchCount()` called automatically
3. Recipe's `fetchCount` increases by 1
4. `GET /api/recipes/trending` shows updated rankings

### Favorites Validation
POST requests validate in this order:
1. ? Request body not null
2. ? RecipeId > 0
3. ? User exists
4. ? Recipe exists
5. ? Not already favorited

### Image URLs
All recipes include HTTPS image URLs from Unsplash, verified in tests:
- `recipe.Img.Should().StartWith("https://")`

---

## Next Steps

- [ ] Update Swagger/OpenAPI spec (if using)
- [ ] Update Postman collection (if using)
- [ ] Update frontend API client types (if using TypeScript)
- [ ] Add these examples to developer onboarding docs

---

*Last Updated: After all 105 tests passed*
*Verified Against: RecipeControllerTests.cs + FavouritesControllerTests.cs*
