import { Person } from './person.model';

export interface AstronautDuty {
  id: number;
  personId: number;
  rank: string;
  dutyTitle: string;
  dutyStartDate: string;
  dutyEndDate: string | null;
}

export interface CreateAstronautDuty {
  name: string;
  rank: string;
  dutyTitle: string;
  dutyStartDate: string;
}

export interface AstronautDutiesResponse {
  person: Person | null;
  astronautDuties: AstronautDuty[];
}

