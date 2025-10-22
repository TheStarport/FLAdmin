import useSwr from "swr";

const fetcher = (url: string) => fetch(url).then((res) => res.json());

function useData() {
  const {
    data: accountData,
    error: accountError,
    isLoading: accountLoading,
  } = useSwr(`/api/getaccounts`, fetcher);

  const {
    data: charactersData,
    error: charactersError,
    isLoading: charactersLoading,
  } = useSwr(`/api/todo-no-endpoint-for-this`, fetcher);

  return {
    accountData,
    accountError,
    accountLoading,
    charactersData,
    charactersError,
    charactersLoading,
  };
}

export default useData;
