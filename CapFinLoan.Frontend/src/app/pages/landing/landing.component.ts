import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-landing',
  imports: [RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css'
})
export class LandingComponent {
  stats = [
    { value: '?50Cr+', label: 'Loans Disbursed' },
    { value: '10,000+', label: 'Happy Customers' },
    { value: '98%', label: 'Approval Rate' },
    { value: '24hrs', label: 'Avg. Processing' }
  ];

  features = [
    { icon: '?', title: 'Lightning-Fast Approval', description: 'Get your loan approved within 24 hours with our AI-powered verification system. No more waiting in long queues.' },
    { icon: '??', title: 'Competitive Rates', description: 'Industry-leading interest rates starting from 8.5% p.a. with zero hidden charges and transparent fee structures.' },
    { icon: '??', title: 'Flexible Tenure', description: 'Choose repayment periods from 6 to 60 months. Customize your EMI to fit your monthly budget perfectly.' }
  ];

  steps = [
    { number: '01', title: 'Create Account', description: 'Sign up in 2 minutes with basic details.' },
    { number: '02', title: 'Submit Application', description: 'Fill in personal, employment & loan details.' },
    { number: '03', title: 'Upload Documents', description: 'Upload ID, income & address proofs.' },
    { number: '04', title: 'Get Funded', description: 'Receive funds directly to your bank.' }
  ];

  products = [
    { icon: '??', title: 'Personal Loan', description: 'For weddings, travel, medical emergencies, or any personal need. Quick disbursal with minimal documentation.', rate: '8.5% p.a.' },
    { icon: '??', title: 'Business Loan', description: 'Fuel your business growth with working capital, equipment financing, or expansion funding.', rate: '10.5% p.a.' },
    { icon: '??', title: 'Home Loan', description: 'Make your dream home a reality with affordable home loans and long tenure options.', rate: '7.5% p.a.' }
  ];

  testimonials = [
    { name: 'Priya Sharma', role: 'Small Business Owner', quote: 'CapFinLoan approved my business loan in just 18 hours. The process was incredibly smooth and transparent. Highly recommended!' },
    { name: 'Rahul Mehta', role: 'IT Professional', quote: 'I was amazed by how easy the entire process was. From application to disbursement, everything was handled digitally.' },
    { name: 'Anita Patel', role: 'Doctor', quote: 'The interest rates are genuinely competitive. I compared multiple lenders and CapFinLoan offered the best deal with no hidden charges.' }
  ];
}
