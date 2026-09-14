export enum ShipmentStatus {
  Draft = 0,
  Scheduled = 1,
  InTransit = 2,
  OutForDelivery = 3,
  Delivered = 4,
  Delayed = 5,
  Cancelled = 6
}

export enum ShipmentPriority {
  Low = 0,
  Normal = 1,
  High = 2,
  Critical = 3
}

export enum WarehouseStatus {
  Active = 0,
  Full = 1,
  Maintenance = 2,
  Inactive = 3
}

export enum IncidentSeverity {
  Minor = 0,
  Moderate = 1,
  Major = 2,
  Critical = 3
}

export enum IncidentStatus {
  Open = 0,
  InInvestigation = 1,
  Mitigated = 2,
  Resolved = 3
}

export enum UserRole {
  Admin = 0,
  LogisticsManager = 1,
  WarehouseOperator = 2,
  Dispatcher = 3
}

export interface Address {
  street: string;
  city: string;
  postalCode: string;
  country: string;
  latitude: number;
  longitude: number;
}

export interface Dimensions {
  lengthCm: number;
  widthCm: number;
  heightCm: number;
  weightKg: number;
  volumeM3: number;
}

export interface ShipmentEvent {
  id: string;
  status: ShipmentStatus;
  location: string;
  description: string;
  timestampUtc: string;
}

export interface Incident {
  id: string;
  shipmentId: string;
  shipmentTrackingNumber: string;
  code: string;
  title: string;
  description: string;
  severity: IncidentSeverity;
  status: IncidentStatus;
  reportedAtUtc: string;
  resolvedAtUtc?: string;
  resolutionNotes?: string;
}

export interface Shipment {
  id: string;
  trackingNumber: string;
  description: string;
  senderName: string;
  recipientName: string;
  originAddress: Address;
  destinationAddress: Address;
  originWarehouseId: string;
  originWarehouseName: string;
  destinationWarehouseId?: string;
  destinationWarehouseName?: string;
  carrierId?: string;
  carrierName?: string;
  status: ShipmentStatus;
  priority: ShipmentPriority;
  dimensions: Dimensions;
  isTemperatureControlled: boolean;
  requiredTemperatureCelsius?: number;
  scheduledPickupDateUtc: string;
  estimatedDeliveryDateUtc: string;
  actualDeliveryDateUtc?: string;
  isDelayed: boolean;
  incidentCount: number;
  createdAtUtc: string;
}

export interface ShipmentDetail extends Shipment {
  events: ShipmentEvent[];
  incidents: Incident[];
}

export interface Warehouse {
  id: string;
  code: string;
  name: string;
  address: Address;
  totalCapacityM3: number;
  usedCapacityM3: number;
  capacityUtilizationPercentage: number;
  status: WarehouseStatus;
  managerName: string;
  contactEmail: string;
  contactPhone: string;
}

export interface Carrier {
  id: string;
  code: string;
  name: string;
  rating: number;
  contactEmail: string;
  contactPhone: string;
  isActive: boolean;
}

export interface DashboardMetrics {
  totalShipments: number;
  inTransitShipments: number;
  deliveredShipments: number;
  delayedShipments: number;
  onTimeDeliveryRatePercentage: number;
  totalWarehouses: number;
  averageWarehouseUtilizationPercentage: number;
  activeIncidentsCount: number;
  recentShipments: Shipment[];
  urgentIncidents: Incident[];
}

export interface User {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
  lastLoginAtUtc?: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresAtUtc: string;
}

export interface CreateShipmentPayload {
  description: string;
  senderName: string;
  recipientName: string;
  originAddress: Address;
  destinationAddress: Address;
  originWarehouseId: string;
  destinationWarehouseId?: string;
  carrierId?: string;
  priority: ShipmentPriority;
  dimensions: {
    lengthCm: number;
    widthCm: number;
    heightCm: number;
    weightKg: number;
  };
  isTemperatureControlled: boolean;
  requiredTemperatureCelsius?: number;
  scheduledPickupDateUtc: string;
  estimatedDeliveryDateUtc: string;
}

export interface UpdateShipmentStatusPayload {
  newStatus: ShipmentStatus;
  location: string;
  reason: string;
}
