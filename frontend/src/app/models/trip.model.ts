import { Bus } from './bus.model';
import { BusRoute } from './route.model';

export interface Trip {
  id: number;
  busId: number;
  routeId: number;
  departureTime: string;
  arrivalTime: string;
  ticketPrice: number;
  platformFee: number;
  totalPrice: number;
  status: string;
  bus?: Bus;
  route?: BusRoute;
}
