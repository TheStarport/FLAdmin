import { Spinner } from "./ui/spinner";

function LoadingDataComponent() {
  return (
    <div className="flex-1 flex justify-center items-center border-2 border-solid gap-2">
      <Spinner />
      <span className="text-muted-foreground">Fetching data...</span>
    </div>
  );
}

export default LoadingDataComponent;
