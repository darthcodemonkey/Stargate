import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PersonService } from '../../services/person.service';
import { AstronautDutyService } from '../../services/astronaut-duty.service';
import { Person } from '../../models/person.model';
import { AstronautDuty } from '../../models/astronaut-duty.model';

@Component({
  selector: 'app-person-detail',
  templateUrl: './person-detail.component.html',
  styleUrl: './person-detail.component.css'
})
export class PersonDetailComponent implements OnInit {
  person: Person | null = null;
  duties: AstronautDuty[] = [];
  loading = false;
  loadingDuties = false;
  error: string | null = null;
  isEditMode = false;
  editName = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private personService: PersonService,
    private astronautDutyService: AstronautDutyService
  ) { }

  ngOnInit(): void {
    const name = this.route.snapshot.paramMap.get('name');
    if (name) {
      const decodedName = decodeURIComponent(name);
      if (decodedName === 'new') {
        this.isEditMode = true;
        this.editName = '';
      } else {
        this.loadPerson(decodedName);
        this.loadDuties(decodedName);
      }
    }
  }

  loadPerson(name: string): void {
    this.loading = true;
    this.error = null;

    this.personService.getPersonByName(name).subscribe({
      next: (person) => {
        this.person = person;
        this.editName = person.name;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load person';
        this.loading = false;
      }
    });
  }

  loadDuties(name: string): void {
    this.loadingDuties = true;

    this.astronautDutyService.getAstronautDutiesByName(name).subscribe({
      next: (response) => {
        this.duties = response.astronautDuties || [];
        if (!this.person && response.person) {
          this.person = response.person;
          this.editName = response.person.name;
        }
        this.loadingDuties = false;
      },
      error: (err) => {
        // If person exists but has no duties, that's OK
        if (err.message.includes('not found')) {
          this.duties = [];
        }
        this.loadingDuties = false;
      }
    });
  }

  toggleEditMode(): void {
    this.isEditMode = !this.isEditMode;
    if (this.person) {
      this.editName = this.person.name;
    }
  }

  savePerson(): void {
    if (!this.editName.trim()) {
      this.error = 'Name is required';
      return;
    }

    this.loading = true;
    this.error = null;

    if (this.person) {
      // Update existing person
      this.personService.updatePerson(this.person.name, { name: this.editName.trim() }).subscribe({
        next: (updatedPerson) => {
          this.person = updatedPerson;
          this.isEditMode = false;
          this.loading = false;
          this.router.navigate(['/persons', encodeURIComponent(updatedPerson.name)]);
        },
        error: (err) => {
          this.error = err.message || 'Failed to update person';
          this.loading = false;
        }
      });
    } else {
      // Create new person
      this.personService.createPerson({ name: this.editName.trim() }).subscribe({
        next: (newPerson) => {
          this.person = newPerson;
          this.isEditMode = false;
          this.loading = false;
          this.router.navigate(['/persons', encodeURIComponent(newPerson.name)]);
        },
        error: (err) => {
          this.error = err.message || 'Failed to create person';
          this.loading = false;
        }
      });
    }
  }

  cancelEdit(): void {
    if (this.person) {
      this.editName = this.person.name;
      this.isEditMode = false;
    } else {
      this.router.navigate(['/persons']);
    }
  }

  goBack(): void {
    this.router.navigate(['/persons']);
  }

  addDuty(): void {
    if (this.person) {
      this.router.navigate(['/persons', encodeURIComponent(this.person.name), 'duties', 'new']);
    }
  }

  getStatusBadge(person: Person): { text: string; class: string } {
    if (person.careerEndDate) {
      return { text: 'Retired', class: 'badge-retired' };
    }
    if (person.currentDutyTitle?.toUpperCase() === 'RETIRED') {
      return { text: 'Retired', class: 'badge-retired' };
    }
    if (person.currentDutyTitle) {
      return { text: 'Active', class: 'badge-active' };
    }
    return { text: 'No Assignment', class: 'badge-inactive' };
  }

  formatDate(dateString: string | null): string {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString();
  }

  isCurrentDuty(duty: AstronautDuty): boolean {
    return duty.dutyEndDate === null;
  }
}

