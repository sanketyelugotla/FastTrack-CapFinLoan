import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ApplicationService } from '../../../core/services/application.service';
import { DocumentService } from '../../../core/services/document.service';
import { LoanApplicationResponse, LoanApplicationStatusResponse } from '../../../core/models/application.models';
import { DocumentResponse } from '../../../core/models/document.models';

@Component({
  selector: 'app-application-detail',
  imports: [RouterLink, DecimalPipe, DatePipe],
  templateUrl: './application-detail.component.html'
})
export class ApplicationDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private appService = inject(ApplicationService);
  private docService = inject(DocumentService);
  
  app = signal<LoanApplicationResponse | null>(null);
  tracking = signal<LoanApplicationStatusResponse | null>(null);
  documents = signal<DocumentResponse[]>([]);

  // Computed signals for Timeline mapping
  isSubmitted = computed(() => !!this.app()?.submittedAtUtc || 
    this.app()?.status === 'DocsPending' || 
    this.app()?.status === 'DocsVerified' || 
    this.app()?.status === 'UnderReview' || 
    this.app()?.status === 'Approved' || 
    this.app()?.status === 'Rejected');
    
  isDocsVerified = computed(() => this.app()?.status === 'DocsVerified' || 
    this.app()?.status === 'UnderReview' || 
    this.app()?.status === 'Approved' || 
    this.app()?.status === 'Rejected');
    
  isUnderReview = computed(() => this.app()?.status === 'UnderReview');
  isDecisionReady = computed(() => this.app()?.status === 'Approved' || this.app()?.status === 'Rejected');

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    
    // Load Application Details
    this.appService.getById(id).subscribe(a => this.app.set(a));
    
    // Load Status Timeline
    this.appService.getStatus(id).subscribe(t => this.tracking.set(t));

    // Load Uploaded Documents
    this.docService.getByApplicationId(id).subscribe(d => this.documents.set(d));
  }
}
