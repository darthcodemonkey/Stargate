import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PersonListComponent } from './components/person-list/person-list.component';
import { PersonDetailComponent } from './components/person-detail/person-detail.component';
import { AstronautDutyFormComponent } from './components/astronaut-duty-form/astronaut-duty-form.component';

const routes: Routes = [
  { path: '', redirectTo: '/persons', pathMatch: 'full' },
  { path: 'persons', component: PersonListComponent },
  { path: 'persons/new', component: PersonDetailComponent },
  { path: 'persons/:name', component: PersonDetailComponent },
  { path: 'persons/:name/duties/new', component: AstronautDutyFormComponent },
  { path: '**', redirectTo: '/persons' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
