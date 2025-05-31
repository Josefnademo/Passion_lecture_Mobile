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

## Scenario 2: Book Reading Experience

1. Select a book from the library
2. Open the book reader view
3. Verify that:
   - The text loads within 3 seconds
   - The progress percentage updates correctly
   - Scrolling is smooth and responsive
   - The last read position is remembered when reopening
   - The book title is displayed in the header

Expected Result: Book content loads quickly and can be read smoothly with proper progress tracking.

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
