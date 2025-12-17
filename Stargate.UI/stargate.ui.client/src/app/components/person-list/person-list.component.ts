import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PersonService } from '../../services/person.service';
import { Person } from '../../models/person.model';

@Component({
  selector: 'app-person-list',
  templateUrl: './person-list.component.html',
  styleUrl: './person-list.component.css'
})
export class PersonListComponent implements OnInit {
  people: Person[] = [];
  loading = false;
  error: string | null = null;
  searchTerm = '';

  constructor(
    private personService: PersonService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadPeople();
  }

  loadPeople(): void {
    this.loading = true;
    this.error = null;

    this.personService.getAllPeople().subscribe({
      next: (people) => {
        this.people = people;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load people';
        this.loading = false;
      }
    });
  }

  viewPerson(person: Person): void {
    this.router.navigate(['/persons', encodeURIComponent(person.name)]);
  }

  addPerson(): void {
    this.router.navigate(['/persons/new']);
  }

  get filteredPeople(): Person[] {
    if (!this.searchTerm.trim()) {
      return this.people;
    }
    
    const term = this.searchTerm.toLowerCase();
    return this.people.filter(person =>
      person.name.toLowerCase().includes(term) ||
      person.currentDutyTitle?.toLowerCase().includes(term) ||
      person.currentRank?.toLowerCase().includes(term)
    );
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
}

