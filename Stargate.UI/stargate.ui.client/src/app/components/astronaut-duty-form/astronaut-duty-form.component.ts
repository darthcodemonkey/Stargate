import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AstronautDutyService } from '../../services/astronaut-duty.service';
import { CreateAstronautDuty } from '../../models/astronaut-duty.model';

@Component({
  selector: 'app-astronaut-duty-form',
  templateUrl: './astronaut-duty-form.component.html',
  styleUrl: './astronaut-duty-form.component.css'
})
export class AstronautDutyFormComponent implements OnInit {
  dutyForm: FormGroup;
  loading = false;
  error: string | null = null;
  personName: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private astronautDutyService: AstronautDutyService
  ) {
    this.dutyForm = this.fb.group({
      rank: ['', [Validators.required, Validators.minLength(1)]],
      dutyTitle: ['', [Validators.required, Validators.minLength(1)]],
      dutyStartDate: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    const name = this.route.snapshot.paramMap.get('name');
    if (name) {
      this.personName = decodeURIComponent(name);
    } else {
      this.error = 'Person name is required';
    }
  }

  onSubmit(): void {
    if (this.dutyForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    this.error = null;

    const formValue = this.dutyForm.value;
    const duty: CreateAstronautDuty = {
      name: this.personName,
      rank: formValue.rank.trim(),
      dutyTitle: formValue.dutyTitle.trim(),
      dutyStartDate: formValue.dutyStartDate
    };

    this.astronautDutyService.createAstronautDuty(duty).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/persons', encodeURIComponent(this.personName)]);
      },
      error: (err) => {
        this.error = err.message || 'Failed to create astronaut duty';
        this.loading = false;
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/persons', encodeURIComponent(this.personName)]);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.dutyForm.controls).forEach(key => {
      const control = this.dutyForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.dutyForm.get(fieldName);
    if (control?.hasError('required') && control?.touched) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }
    if (control?.hasError('minlength') && control?.touched) {
      return `${this.getFieldLabel(fieldName)} must be at least 1 character`;
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      rank: 'Rank',
      dutyTitle: 'Duty Title',
      dutyStartDate: 'Start Date'
    };
    return labels[fieldName] || fieldName;
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.dutyForm.get(fieldName);
    return !!(control && control.invalid && control.touched);
  }
}

