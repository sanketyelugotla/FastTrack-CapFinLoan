# CapFinLoan Use Case Diagram

Because PDF files cannot be directly edited, this document contains a Mermaid JS syntax representation of the CapFinLoan Use Case Diagram. You can view this diagram visually by using any Markdown previewer that supports Mermaid (like GitHub, or VS Code extensions).

```mermaid
usecaseDiagram
  actor Applicant as "Applicant (Customer)"
  actor Administrator as "Administrator (Loan Officer)"

  package "CapFinLoan System" {
    
    usecase "Register Account" as UC1
    usecase "Login / OAuth" as UC2
    usecase "Calculate Loan EMI" as UC3
    usecase "Submit Loan Application" as UC4
    usecase "Upload Verification Documents" as UC5
    usecase "Track Application Status" as UC6
    usecase "Re-upload Rejected Documents" as UC7

    usecase "Review Submitted Applications" as UC8
    usecase "Verify Identity & Income Proofs" as UC9
    usecase "Approve/Reject Loan" as UC10
    usecase "Generate PDF Reports" as UC11
    usecase "View Audit Logs" as UC12

    Applicant --> UC1
    Applicant --> UC2
    Applicant --> UC3
    Applicant --> UC4
    Applicant --> UC5
    Applicant --> UC6
    Applicant --> UC7

    Administrator --> UC2
    Administrator --> UC8
    Administrator --> UC9
    Administrator --> UC10
    Administrator --> UC11
    Administrator --> UC12

    UC4 ..> UC5 : <<includes>>
    UC10 ..> UC9 : <<includes>>
  }
```

### Key Use Case Explanations:
1. **Submit Loan Application (Applicant):** The applicant fills in their personal, contact, and employment details, requesting a specific loan amount. This transitions the application from Draft to Submitted.
2. **Upload Verification Documents (Applicant):** Driven by the Document Service, the user uploads PDFs or Images of their identity, address, and income proofs.
3. **Verify Identity & Income Proofs (Administrator):** The admin checks the uploaded documents and marks them either 'Verified' or 'ReuploadRequired'.
4. **Approve/Reject Loan (Administrator):** Once all documents are verified, the admin makes a final decision on the application. This automatically locks the application and documents from further edits and dispatches a notification to the user.
