# Manual Test Scenarios for EPUB Reader Application

## Scenario 1: Book Upload and Management

1. Launch the application
2. Navigate to the book upload section
3. Select an EPUB file from the device storage
4. Verify that:
   - The book appears in the library
   - The upload date is correct
   - The cover image is displayed
   - The book title is extracted correctly

Expected Result: Book should be successfully uploaded and displayed in the library with all metadata.

## Scenario 2: Tag Management and Filtering

1. Create a new tag called "Science Fiction"
2. Create another tag called "Classics"
3. Select a book from the library
4. Associate both tags with the book
5. Go to the library view
6. Filter books by "Science Fiction" tag
7. Verify that:
   - Only books with the "Science Fiction" tag are shown
   - The book count matches the filter
   - Clearing the filter shows all books again

Expected Result: Tags should be created, associated with books, and filtering should work correctly.

## Scenario 3: Reading Progress and Navigation

1. Open any book from the library
2. Read through several pages using the navigation buttons
3. Close the book
4. Reopen the application
5. Open the same book
6. Verify that:
   - The book opens to the last read page
   - Navigation buttons work correctly
   - Page numbers are consistent
   - The reading progress is saved when closing the book

Expected Result: Reading progress should be maintained between sessions, and navigation should be smooth and reliable.
