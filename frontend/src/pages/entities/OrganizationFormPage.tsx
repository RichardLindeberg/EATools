import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useController, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { OrganizationFormSchema, OrganizationFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DynamicFieldArray } from '../../components/forms/DynamicFieldArray';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import { updateOrganizationWithCommands } from '../../utils/commandDispatcher';
import './EntityFormPage.css';

interface OrganizationFormPageProps {
  isEdit?: boolean;
}

export function OrganizationFormPage({ isEdit = false }: OrganizationFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);
  const [originalValues, setOriginalValues] = useState<OrganizationFormData | null>(null);

  const { data: existingOrganization, isLoading: isLoadingOrganization } = useEntity(
    isEdit ? 'organizations' : '',
    isEdit ? id : ''
  );

  const {
    register,
    control,
    watch,
    handleSubmit: handleReactFormSubmit,
    formState: { errors, isSubmitting },
    setError,
    reset,
  } = useForm<OrganizationFormData>({
    resolver: zodResolver(OrganizationFormSchema),
    defaultValues: isEdit
      ? existingOrganization || {}
      : {
          name: '',
          description: '',
          owner: '',
          status: 'Active',
          type: 'Department',
          parent: null,
          tags: [],
        },
  });

  const { field: tagsField } = useController({
    control,
    name: 'tags',
  });

  const watchedValues = watch();
  const hasChanges = originalValues ? hasUnsavedChanges(originalValues, watchedValues) : false;

  useEffect(() => {
    if (existingOrganization) {
      reset(existingOrganization);
      setOriginalValues(deepClone(existingOrganization as OrganizationFormData));
    }
  }, [existingOrganization, reset]);

  useEffect(() => {
    if (!isEdit) {
      const defaults: OrganizationFormData = {
        name: '',
        description: '',
        owner: '',
        status: 'Active',
        type: 'Department',
        parent: null,
        tags: [],
      };
      setOriginalValues(defaults);
    }
  }, [isEdit]);

  const onSubmit = async (data: OrganizationFormData) => {
    try {
      let response;

      if (isEdit && id) {
        const result = await updateOrganizationWithCommands(
          id,
          originalValues || {},
          data
        );
        if (result?.id) {
          navigate(`/entities/organizations/${result.id}`);
        }
      } else {
        response = await apiClient.post('/organizations', data);
        if (response.data?.id) {
          navigate(`/entities/organizations/${response.data.id}`);
        }
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save organization';

      if (error?.response?.status === 422 && error?.response?.data?.errors) {
        error.response.data.errors.forEach((err: any) => {
          if (err.field) {
            setError(err.field as any, {
              type: 'server',
              message: err.message,
            });
          }
        });
      }
    }
  };

  const handleCancel = () => {
    if (hasChanges) {
      setShowDiscardModal(true);
    } else {
      navigate('/entities/organizations');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/organizations');
  };

  if (isEdit && isLoadingOrganization) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Organization' : 'Create Organization'}
        subtitle={
          isEdit ? `Editing: ${existingOrganization?.name || ''}` : 'Add a new organization'
        }
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Organization'}
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Basic Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Organization Name"
              required
              error={errors.name?.message}
              htmlFor="name"
            >
              <input
                id="name"
                type="text"
                placeholder="e.g., Engineering Department"
                {...register('name')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Type"
              error={errors.type?.message}
              htmlFor="type"
            >
              <select id="type" {...register('type')}>
                <option value="Department">Department</option>
                <option value="Team">Team</option>
                <option value="Division">Division</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Owner"
              required
              error={errors.owner?.message}
              htmlFor="owner"
            >
              <input
                id="owner"
                type="text"
                placeholder="Enter owner user ID"
                {...register('owner')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Status"
              required
              error={errors.status?.message}
              htmlFor="status"
            >
              <select id="status" {...register('status')}>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="Pending">Pending</option>
              </select>
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Description"
            error={errors.description?.message}
            htmlFor="description"
          >
            <textarea
              id="description"
              placeholder="Describe the organization..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Hierarchy</h3>

          <FormFieldWrapper
            label="Parent Organization"
            error={errors.parent?.message}
            helpText="Leave empty for top-level organization"
            htmlFor="parent"
          >
            <input
              id="parent"
              type="text"
              placeholder="Parent organization ID (optional)"
              {...register('parent')}
            />
          </FormFieldWrapper>

          <DynamicFieldArray
            label="Tags"
            value={tagsField.value || []}
            onChange={tagsField.onChange}
            placeholder="e.g., engineering, frontend"
            error={errors.tags?.message}
          />
        </div>
      </EntityFormTemplate>

      <DiscardChangesModal
        isOpen={showDiscardModal}
        onConfirm={handleConfirmDiscard}
        onCancel={() => setShowDiscardModal(false)}
      />
    </>
  );
}
